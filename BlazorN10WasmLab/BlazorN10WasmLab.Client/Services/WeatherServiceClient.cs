using BlazorN10WasmLab.Contracts;
using Grpc.Core;
using Grpc.Net.Client;
using ProtoBuf;
using ProtoBuf.Grpc;

namespace BlazorN10WasmLab.Client.Services;

// 具體實作取代動態 proxy：Blazor WASM 不支援 Reflection.Emit，
// 無法使用 channel.CreateGrpcService<T>()，必須手動實作 CallInvoker 呼叫。
public sealed class WeatherServiceClient : IWeatherService
{
    // protobuf-net.Grpc 的 service 命名規則：
    //   type.Namespace + "." + type.Name（介面去掉開頭 'I'）
    private const string ServiceName = "BlazorN10WasmLab.Contracts.WeatherService";

    private static readonly Marshaller<GrpcEmpty> EmptyMarshaller = CreateMarshaller<GrpcEmpty>();
    private static readonly Marshaller<WeatherForecastReply> ReplyMarshaller = CreateMarshaller<WeatherForecastReply>();
    private static readonly Marshaller<WeatherForecast> ForecastMarshaller = CreateMarshaller<WeatherForecast>();

    // protobuf-net.Grpc 的 method 命名規則：method name 去掉 "Async" 後綴
    private static readonly Method<GrpcEmpty, WeatherForecastReply> GetForecastsMethod =
        new(MethodType.Unary, ServiceName, "GetForecasts", EmptyMarshaller, ReplyMarshaller);

    private static readonly Method<GrpcEmpty, WeatherForecast> StreamForecastsMethod =
        new(MethodType.ServerStreaming, ServiceName, "StreamForecasts", EmptyMarshaller, ForecastMarshaller);

    private readonly CallInvoker _invoker;

    public WeatherServiceClient(GrpcChannel channel) => _invoker = channel.CreateCallInvoker();

    public async ValueTask<WeatherForecastReply> GetForecastsAsync(CallContext context = default)
    {
        var call = _invoker.AsyncUnaryCall(GetForecastsMethod, null,
            new CallOptions(cancellationToken: context.CancellationToken), GrpcEmpty.Instance);
        return await call.ResponseAsync;
    }

    public async IAsyncEnumerable<WeatherForecast> StreamForecastsAsync(CallContext context = default)
    {
        var call = _invoker.AsyncServerStreamingCall(StreamForecastsMethod, null,
            new CallOptions(cancellationToken: context.CancellationToken), GrpcEmpty.Instance);

        while (await call.ResponseStream.MoveNext(context.CancellationToken))
        {
            yield return call.ResponseStream.Current;
        }
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

// protobuf-net.Grpc 對無參數方法使用空 ProtoContract 作為 request type。
// 空 protobuf 訊息序列化為 0 bytes，與 server 端的 internal Empty 完全相容。
[ProtoContract]
internal sealed class GrpcEmpty
{
    public static readonly GrpcEmpty Instance = new();
}
