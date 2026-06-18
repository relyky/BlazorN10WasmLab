#pragma warning disable OPENAI001 // OpenAI.Responses 型別標 [Experimental];本專案鎖定 2.11.0 直接使用。
using System.Runtime.CompilerServices;
using System.Text.Json;
using OpenAI.Responses;

namespace BlazorN10WasmLab.Services;

/// <summary>
/// 一次 LLM 工具呼叫(模型決定要呼叫哪個本地函式、帶什麼參數)。
/// </summary>
public sealed record ToolCall(string CallId, string Name, string ArgumentsJson);

/// <summary>
/// 由 <see cref="ChatResponder"/> 串流吐出的事件,供 UI 即時渲染。
/// </summary>
public abstract record ChatStreamEvent
{
    /// <summary>一段新到達的回答文字(打字機效果)。</summary>
    public sealed record TextDelta(string Text) : ChatStreamEvent;

    /// <summary>模型發起一次工具呼叫(此時尚未執行)。供 UI 顯示「Searching…」等卡片。</summary>
    public sealed record ToolCallStarted(ToolCall Call) : ChatStreamEvent;
}

/// <summary>
/// 一個可供模型呼叫的工具:定義(名稱/描述/參數 schema)+ 實際執行的委派。
/// </summary>
public sealed class ChatTool
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    /// <summary>JSON Schema(描述參數),作為 ResponseTool.CreateFunctionTool 的 functionParameters。</summary>
    public required string ParametersJsonSchema { get; init; }
    /// <summary>執行工具:收到模型給的參數 JSON,回傳要餵回模型的字串結果。</summary>
    public required Func<string, Task<string>> InvokeAsync { get; init; }
}

/// <summary>
/// 封裝與 LLM 的溝通(原生 OpenAI Responses API)。
/// 自寫多輪 tool-call 串流狀態機,取代 Microsoft.Extensions.AI 的 UseFunctionInvocation。
/// </summary>
public sealed class ChatResponder(ResponsesClient client)
{
    public const string Model = "gpt-5.1";

    /// <summary>
    /// 串流一回合對話(可能跨多輪 tool call)。
    ///
    /// 流程:CreateResponseStreamingAsync → 逐 update 吐出 TextDelta / ToolCallStarted →
    /// 串流結束若有 tool call:執行對應 ChatTool,將結果以 FunctionCallOutputItem 帶
    /// previous_response_id 發起下一輪串流 → 直到模型不再要求工具。
    /// </summary>
    /// <param name="userInput">本回合使用者輸入。</param>
    /// <param name="instructions">系統指示(每回合重送;Responses API 的 instructions 不跨回合保留)。</param>
    /// <param name="tools">可供模型呼叫的工具。</param>
    /// <param name="previousResponseId">
    /// 上一回合的 response id(server 端對話狀態);首回合為 null。
    /// 透過 out 參數回傳本回合最終 response id,供呼叫端鏈接下一回合。
    /// </param>
    public async IAsyncEnumerable<ChatStreamEvent> StreamReplyAsync(
        string userInput,
        string instructions,
        IReadOnlyList<ChatTool> tools,
        string? previousResponseId,
        Action<string> onFinalResponseId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var toolsByName = tools.ToDictionary(t => t.Name);
        var responseTools = tools
            .Select(t => ResponseTool.CreateFunctionTool(
                functionName: t.Name,
                functionParameters: BinaryData.FromString(t.ParametersJsonSchema),
                strictModeEnabled: false,
                functionDescription: t.Description))
            .ToList();

        // 首輪送 user input;後續輪只送 function 結果(靠 previous_response_id 串接歷史)。
        var inputItems = new List<ResponseItem> { ResponseItem.CreateUserMessageItem(userInput) };
        var currentPreviousId = previousResponseId;

        while (true)
        {
            var options = new CreateResponseOptions
            {
                Model = Model,
                Instructions = instructions,
                StreamingEnabled = true,
                StoredOutputEnabled = true,
                PreviousResponseId = currentPreviousId,
                // gpt-5.x reasoning 模型不可設 Temperature,沿用模型預設。
            };
            foreach (var item in inputItems)
                options.InputItems.Add(item);
            foreach (var tool in responseTools)
                options.Tools.Add(tool);

            var pendingCalls = new List<ToolCall>();
            string? responseId = null;

            await foreach (var update in client.CreateResponseStreamingAsync(options, cancellationToken))
            {
                switch (update)
                {
                    case StreamingResponseOutputTextDeltaUpdate delta when delta.Delta is { Length: > 0 }:
                        yield return new ChatStreamEvent.TextDelta(delta.Delta);
                        break;

                    // item done 時 function call 的參數已完整到齊,毋須自行拼裝 delta 分片。
                    case StreamingResponseOutputItemDoneUpdate { Item: FunctionCallResponseItem fc }:
                        var call = new ToolCall(fc.CallId, fc.FunctionName, fc.FunctionArguments.ToString());
                        pendingCalls.Add(call);
                        yield return new ChatStreamEvent.ToolCallStarted(call);
                        break;

                    case StreamingResponseCompletedUpdate completed:
                        responseId = completed.Response.Id;
                        break;
                }
            }

            // 本輪 response id 永遠回報給呼叫端(供下一回合鏈接 / New chat 前的狀態)。
            if (responseId is not null)
                onFinalResponseId(responseId);

            if (pendingCalls.Count == 0)
                yield break; // 模型不再要工具,本回合結束

            // 執行所有 tool call,把結果作為下一輪的 input,並把 previous_response_id 指向本輪。
            currentPreviousId = responseId;
            inputItems = new List<ResponseItem>();
            foreach (var call in pendingCalls)
            {
                var output = toolsByName.TryGetValue(call.Name, out var tool)
                    ? await tool.InvokeAsync(call.ArgumentsJson)
                    : $"Unknown tool '{call.Name}'.";
                inputItems.Add(ResponseItem.CreateFunctionCallOutputItem(call.CallId, output));
            }
        }
    }

    // 追問建議用的 Structured Outputs schema:根必須是 object,故包一層 suggestions 陣列。
    private static readonly BinaryData SuggestionsSchema = BinaryData.FromString("""
        {"type":"object","properties":{"suggestions":{"type":"array","items":{"type":"string"}}},"required":["suggestions"],"additionalProperties":false}
        """);

    /// <summary>
    /// 一次性、無狀態的結構化輸出:依對話脈絡產生 ≤3 條追問建議(取代 M.E.AI 的 GetResponseAsync&lt;string[]&gt;)。
    /// 非串流、不帶工具、不鏈接 previous_response_id。任何例外或空結果回傳空陣列。
    /// </summary>
    /// <param name="contextLines">已縮減的對話脈絡(role + text),依序組成 user input。</param>
    /// <param name="prompt">產生建議的指示(放 Instructions)。</param>
    public async Task<string[]> SuggestAsync(
        IEnumerable<string> contextLines,
        string prompt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new CreateResponseOptions
            {
                Model = Model,
                Instructions = prompt,
                TextOptions = new ResponseTextOptions
                {
                    TextFormat = ResponseTextFormat.CreateJsonSchemaFormat(
                        jsonSchemaFormatName: "follow_up_suggestions",
                        jsonSchema: SuggestionsSchema,
                        jsonSchemaFormatDescription: "Up to 3 follow-up question suggestions.",
                        jsonSchemaIsStrict: true),
                },
            };
            options.InputItems.Add(ResponseItem.CreateUserMessageItem(string.Join("\n", contextLines)));

            var response = await client.CreateResponseAsync(options, cancellationToken);

            var json = ExtractText(response.Value);
            if (string.IsNullOrWhiteSpace(json))
                return [];

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("suggestions", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                return arr.EnumerateArray()
                    .Select(e => e.GetString())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s!)
                    .ToArray();
            }
            return [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return [];
        }
    }

    // 遍歷 OutputItems,串接 message item 各 content part 的文字(非訊息型 item 略過)。
    private static string ExtractText(ResponseResult response)
        => string.Concat(response.OutputItems
            .OfType<MessageResponseItem>()
            .SelectMany(m => m.Content)
            .Select(p => p.Text)
            .Where(t => !string.IsNullOrEmpty(t)));
}
