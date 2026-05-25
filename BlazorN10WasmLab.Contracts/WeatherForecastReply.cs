using ProtoBuf;

namespace BlazorN10WasmLab.Contracts;

[ProtoContract]
public class WeatherForecastReply
{
    [ProtoMember(1)]
    public List<WeatherForecast> Forecasts { get; set; } = [];
}
