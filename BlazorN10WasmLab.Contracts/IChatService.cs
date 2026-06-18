using ProtoBuf.Grpc;
using System.ServiceModel;

namespace BlazorN10WasmLab.Contracts;

[ServiceContract]
public interface IChatService
{
    /// <summary>串流一回合對話（可能跨多輪 tool call）；工具呼叫迴圈在 Server 端執行。</summary>
    IAsyncEnumerable<ChatStreamReply> StreamReplyAsync(ChatStreamRequest request, CallContext context = default);

    /// <summary>依對話脈絡產生 ≤3 條追問建議（一次性、無狀態）。</summary>
    ValueTask<SuggestionsReply> SuggestAsync(SuggestRequest request, CallContext context = default);
}
