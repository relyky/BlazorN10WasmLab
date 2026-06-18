using BlazorN10WasmLab.Contracts;
using Grpc.Core;
using Grpc.Net.Client;
using ProtoBuf;
using ProtoBuf.Grpc;

namespace BlazorN10WasmLab.Client.Services;

// 具體實作取代動態 proxy（WASM 不支援 Reflection.Emit）。仿 WeatherServiceClient。
public sealed class ChatServiceClient : IChatService
{
    // 命名規則：Namespace + "." + 介面去開頭 'I'；method 去 "Async" 後綴。
    private const string ServiceName = "BlazorN10WasmLab.Contracts.ChatService";

    private static readonly Marshaller<ChatStreamRequest> StreamRequestMarshaller = CreateMarshaller<ChatStreamRequest>();
    private static readonly Marshaller<ChatStreamReply> StreamReplyMarshaller = CreateMarshaller<ChatStreamReply>();
    private static readonly Marshaller<SuggestRequest> SuggestRequestMarshaller = CreateMarshaller<SuggestRequest>();
    private static readonly Marshaller<SuggestionsReply> SuggestionsReplyMarshaller = CreateMarshaller<SuggestionsReply>();

    private static readonly Method<ChatStreamRequest, ChatStreamReply> StreamReplyMethod =
        new(MethodType.ServerStreaming, ServiceName, "StreamReply", StreamRequestMarshaller, StreamReplyMarshaller);

    private static readonly Method<SuggestRequest, SuggestionsReply> SuggestMethod =
        new(MethodType.Unary, ServiceName, "Suggest", SuggestRequestMarshaller, SuggestionsReplyMarshaller);

    private readonly CallInvoker _invoker;

    public ChatServiceClient(GrpcChannel channel) => _invoker = channel.CreateCallInvoker();

    public async IAsyncEnumerable<ChatStreamReply> StreamReplyAsync(
        ChatStreamRequest request,
        CallContext context = default)
    {
        using var call = _invoker.AsyncServerStreamingCall(StreamReplyMethod, null,
            new CallOptions(cancellationToken: context.CancellationToken), request);

        while (await call.ResponseStream.MoveNext(context.CancellationToken))
        {
            yield return call.ResponseStream.Current;
        }
    }

    public async ValueTask<SuggestionsReply> SuggestAsync(SuggestRequest request, CallContext context = default)
    {
        using var call = _invoker.AsyncUnaryCall(SuggestMethod, null,
            new CallOptions(cancellationToken: context.CancellationToken), request);
        return await call.ResponseAsync;
    }

    private static Marshaller<T> CreateMarshaller<T>() =>
        Marshallers.Create<T>(
            msg =>
            {
                using var ms = new MemoryStream();
                Serializer.Serialize(ms, msg);
                return ms.ToArray();
            },
            bytes => Serializer.Deserialize<T>(new MemoryStream(bytes))
        );
}
