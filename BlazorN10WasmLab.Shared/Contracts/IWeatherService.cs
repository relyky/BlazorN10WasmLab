using ProtoBuf.Grpc;
using System.ServiceModel;

namespace BlazorN10WasmLab.Shared.Contracts;

[ServiceContract]
public interface IWeatherService
{
    ValueTask<WeatherForecastReply> GetForecastsAsync(CallContext context = default);

    IAsyncEnumerable<WeatherForecast> StreamForecastsAsync(CallContext context = default);
}
