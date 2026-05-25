using ProtoBuf;

namespace BlazorN10WasmLab.Contracts;

[ProtoContract]
public class WeatherForecast
{
    [ProtoMember(1)]
    public DateOnly Date { get; set; }

    [ProtoMember(2)]
    public int TemperatureC { get; set; }

    [ProtoMember(3)]
    public string? Summary { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
