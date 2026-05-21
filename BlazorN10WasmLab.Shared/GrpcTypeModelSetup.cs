using BlazorN10WasmLab.Shared.Surrogates;
using ProtoBuf.Meta;

namespace BlazorN10WasmLab.Shared;

public static class GrpcTypeModelSetup
{
    public static void Register()
    {
        RuntimeTypeModel.Default
            .Add(typeof(DateOnly), false)
            .SetSurrogate(typeof(DateOnlySurrogate));
    }
}
