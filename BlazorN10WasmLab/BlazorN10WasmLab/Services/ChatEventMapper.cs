using System.Text.Json;
using BlazorN10WasmLab.Contracts;

namespace BlazorN10WasmLab.Services;

/// <summary>
/// 把內部串流事件 <see cref="ChatStreamEvent"/> 映射成扁平的 gRPC 回覆 <see cref="ChatStreamReply"/>。
/// 純函式：不依賴 OpenAI / 串流。Search 工具參數解析搬自來源 Chat.razor 的 ToMarker。
/// </summary>
public static class ChatEventMapper
{
    public static ChatStreamReply Map(ChatStreamEvent ev) => ev switch
    {
        ChatStreamEvent.TextDelta textDelta => new ChatStreamReply
        {
            Kind = ChatEventKind.TextDelta,
            TextDelta = textDelta.Text,
        },
        ChatStreamEvent.ToolCallStarted started => MapToolCall(started.Call),
        _ => new ChatStreamReply { Kind = ChatEventKind.TextDelta, TextDelta = string.Empty },
    };

    public static ChatStreamReply ResponseId(string responseId) => new()
    {
        Kind = ChatEventKind.ResponseId,
        ResponseId = responseId,
    };

    private static ChatStreamReply MapToolCall(ToolCall call)
    {
        var reply = new ChatStreamReply
        {
            Kind = ChatEventKind.ToolCallStarted,
            ToolName = call.Name,
        };

        if (call.Name == "Search")
        {
            try
            {
                using var doc = JsonDocument.Parse(call.ArgumentsJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("searchPhrase", out var p))
                    reply.SearchPhrase = p.GetString();
                if (root.TryGetProperty("filenameFilter", out var f))
                    reply.FilenameFilter = f.GetString();
            }
            catch (JsonException)
            {
                // 參數無法解析時僅保留 ToolName。
            }
        }

        return reply;
    }
}
