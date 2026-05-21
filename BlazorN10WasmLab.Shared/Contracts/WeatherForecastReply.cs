using ProtoBuf;

namespace BlazorN10WasmLab.Shared.Contracts;

[ProtoContract]
public class WeatherForecastReply
{
    [ProtoMember(1)]
    public List<WeatherForecast> Forecasts { get; set; } = [];
}
