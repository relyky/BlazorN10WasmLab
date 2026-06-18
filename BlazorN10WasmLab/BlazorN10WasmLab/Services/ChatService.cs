using System.Text.Json;
using BlazorN10WasmLab.Contracts;
using ProtoBuf.Grpc;

namespace BlazorN10WasmLab.Services;

/// <summary>
/// 聊天 gRPC 服務：工具呼叫迴圈完整在 Server 端執行（取代來源 Chat.razor 的畫面端編排）。
/// 內部組裝 LoadDocuments / Search 兩個工具（呼叫 SemanticSearch），跑 ChatResponder.StreamReplyAsync，
/// 把事件經 ChatEventMapper 映射成扁平 ChatStreamReply 串流給前端。
/// </summary>
public sealed class ChatService(ChatResponder responder, SemanticSearch search) : IChatService
{
    // 系統提示詞與工具定義搬自來源 Chat.razor。
    private const string SystemPrompt = @"
        You are an assistant who answers questions about information you retrieve.
        Do not answer questions about anything else.
        Use only simple markdown to format your responses.

        Use the LoadDocuments tool to prepare for searches before answering any questions.

        Use the Search tool to find relevant information. When you do this, end your
        reply with citations in the special XML format:

        <citation filename='string'>exact quote here</citation>

        Always include the citation in your response if there are results.

        The quote must be max 5 words, taken word-for-word from the search result, and is the basis for why the citation is relevant.
        Don't refer to the presence of citations; just emit these tags right at the end, with no surrounding text.
        ";

    // 追問建議提示詞搬自來源 ChatSuggestions.razor。
    private const string SuggestPrompt = @"
        Suggest up to 3 follow-up questions that I could ask you to help me complete my task.
        Each suggestion must be a complete sentence, maximum 6 words.
        Each suggestion must be phrased as something that I (the user) would ask you (the assistant) in response to your previous message,
        for example 'How do I do that?' or 'Explain ...'.
        If there are no suggestions, reply with an empty list.
        ";

    public async IAsyncEnumerable<ChatStreamReply> StreamReplyAsync(
        ChatStreamRequest request,
        CallContext context = default)
    {
        var tools = BuildTools();
        var cancellationToken = context.CancellationToken;

        string? finalResponseId = null;

        await foreach (var ev in responder.StreamReplyAsync(
            request.UserInput,
            SystemPrompt,
            tools,
            request.PreviousResponseId,
            id => finalResponseId = id,
            cancellationToken))
        {
            yield return ChatEventMapper.Map(ev);
        }

        // 回合最終 response id 以串流事件回傳（取代來源 onFinalResponseId 回呼），供前端鏈接下一回合。
        if (finalResponseId is not null)
            yield return ChatEventMapper.ResponseId(finalResponseId);
    }

    public async ValueTask<SuggestionsReply> SuggestAsync(
        SuggestRequest request,
        CallContext context = default)
    {
        var suggestions = await responder.SuggestAsync(request.ContextLines, SuggestPrompt, context.CancellationToken);
        return new SuggestionsReply { Suggestions = [.. suggestions] };
    }

    // 組裝 LoadDocuments / Search 兩個工具（搬自來源 Chat.razor.OnInitialized）。
    private IReadOnlyList<ChatTool> BuildTools() =>
    [
        new ChatTool
        {
            Name = "LoadDocuments",
            Description = "Loads the documents needed for performing searches. Must be completed before a search can be executed, but only needs to be completed once.",
            ParametersJsonSchema = """{"type":"object","properties":{},"additionalProperties":false}""",
            InvokeAsync = async _ =>
            {
                await search.LoadDocumentsAsync();
                return "Documents loaded.";
            },
        },
        new ChatTool
        {
            Name = "Search",
            Description = "Searches for information using a phrase or keyword. Relies on documents already being loaded.",
            ParametersJsonSchema = """
                {"type":"object","properties":{"searchPhrase":{"type":"string","description":"The phrase to search for."},"filenameFilter":{"type":"string","description":"If possible, specify the filename to search that file only. If not provided or empty, the search includes all files."}},"required":["searchPhrase"]}
                """,
            InvokeAsync = SearchToolAsync,
        },
    ];

    // Search 工具：解析參數 → 查 SemanticSearch → 結果包成 <result> 字串回給模型（搬自來源 SearchToolAsync）。
    private async Task<string> SearchToolAsync(string argumentsJson)
    {
        string searchPhrase = "";
        string? filenameFilter = null;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;
            if (root.TryGetProperty("searchPhrase", out var p)) searchPhrase = p.GetString() ?? "";
            if (root.TryGetProperty("filenameFilter", out var f)) filenameFilter = f.GetString();
        }
        catch (JsonException)
        {
        }

        var results = await search.SearchAsync(searchPhrase, filenameFilter, maxResults: 5);
        return string.Join("\n", results.Select(r => $"<result filename=\"{r.DocumentId}\">{r.Text}</result>"));
    }
}
