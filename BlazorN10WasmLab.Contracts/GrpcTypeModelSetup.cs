using BlazorN10WasmLab.Contracts.Surrogates;
using ProtoBuf.Meta;

namespace BlazorN10WasmLab.Contracts;

public static class GrpcTypeModelSetup
{
    public static void Register()
    {
        RuntimeTypeModel.Default
            .Add(typeof(DateOnly), false)
            .SetSurrogate(typeof(DateOnlySurrogate));
    }
}
