using ProtoBuf;

namespace BlazorN10WasmLab.Shared.Surrogates;

[ProtoContract]
public struct DateOnlySurrogate
{
    [ProtoMember(1)]
    public int DayNumber { get; set; }

    public static implicit operator DateOnly(DateOnlySurrogate surrogate)
        => DateOnly.FromDayNumber(surrogate.DayNumber);

    public static implicit operator DateOnlySurrogate(DateOnly date)
        => new() { DayNumber = date.DayNumber };
}
