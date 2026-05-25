namespace BlazorN10WasmLab.Client.Services;

public interface IViewportBreakpoint
{
    bool IsMobile { get; }

    event Action OnChanged;

    ValueTask EnsureInitializedAsync();
}
