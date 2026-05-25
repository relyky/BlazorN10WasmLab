using Microsoft.JSInterop;

namespace BlazorN10WasmLab.Client.Services;

public sealed class ViewportBreakpoint : IViewportBreakpoint, IAsyncDisposable
{
    // 對齊 Tailwind md: 斷點反向（md 從 >=768px 起算，mobile 為 <768）
    private const string MobileMediaQuery = "(max-width: 767.98px)";

    private readonly IJSRuntime _js;
    private IJSObjectReference? _module;
    private DotNetObjectReference<ViewportBreakpoint>? _selfRef;
    private bool _initialized;

    public ViewportBreakpoint(IJSRuntime js) => _js = js;

    public bool IsMobile { get; private set; }

    public event Action? OnChanged;

    public async ValueTask EnsureInitializedAsync()
    {
        if (_initialized) return;

        _module = await _js.InvokeAsync<IJSObjectReference>(
            "import", "./js/viewport-breakpoint.js");
        _selfRef = DotNetObjectReference.Create(this);
        IsMobile = await _module.InvokeAsync<bool>("init", MobileMediaQuery, _selfRef);
        _initialized = true;
    }

    [JSInvokable]
    public void OnMatchChanged(bool isMobile)
    {
        if (IsMobile == isMobile) return;
        IsMobile = isMobile;
        OnChanged?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try { await _module.InvokeVoidAsync("dispose"); }
            catch (JSDisconnectedException) { /* tab closing */ }
            await _module.DisposeAsync();
        }
        _selfRef?.Dispose();
    }
}
