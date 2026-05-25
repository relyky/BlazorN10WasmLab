## 1. 新增 viewport breakpoint 服務

- [x] 1.1 新增 `BlazorN10WasmLab/BlazorN10WasmLab.Client/Services/IViewportBreakpoint.cs`：定義介面 `bool IsMobile`、`event Action OnChanged`、`ValueTask EnsureInitializedAsync()`
- [x] 1.2 新增 `BlazorN10WasmLab/BlazorN10WasmLab.Client/Services/ViewportBreakpoint.cs`：實作 `IViewportBreakpoint, IAsyncDisposable`；持有 `IJSObjectReference` (JS module) 與 `DotNetObjectReference<ViewportBreakpoint>`；`EnsureInitializedAsync` 內 `IJSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/viewport-breakpoint.js")`、呼叫 `init('(max-width: 767.98px)', dotNetRef)` 取得初始 matches 並儲存；`[JSInvokable] OnMatchChanged(bool isMobile)` 更新 `IsMobile` 並 raise `OnChanged`；`DisposeAsync` 呼叫 module dispose
- [x] 1.3 新增 `BlazorN10WasmLab/BlazorN10WasmLab.Client/wwwroot/js/viewport-breakpoint.js`：export `init(mediaQuery, dotNetRef)` 與 `dispose(handle)`，dumb adapter 不寫死任何字串

## 2. DI 註冊與 layout 改寫

- [x] 2.1 `BlazorN10WasmLab/BlazorN10WasmLab.Client/Program.cs` 加 `builder.Services.AddSingleton<IViewportBreakpoint, ViewportBreakpoint>();`
- [x] 2.2 `BlazorN10WasmLab/BlazorN10WasmLab.Client/Layout/MainLayout.razor`：移除 `@inject IJSRuntime JS`（若無其他用途）、改加 `@inject IViewportBreakpoint Viewport` 與 `@implements IDisposable`
- [x] 2.3 `MainLayout` `OnAfterRenderAsync(firstRender)`：移除 `eval('window.innerWidth')`，改為 `await Viewport.EnsureInitializedAsync()`、依 `Viewport.IsMobile` 設 `_sidebarOpen`、訂閱 `Viewport.OnChanged += HandleViewportChanged`
- [x] 2.4 `MainLayout` 新增 `HandleViewportChanged()`：mobile → `_sidebarOpen = false`；desktop → `_sidebarOpen = true`；以 `InvokeAsync(StateHasChanged)` 通知重新渲染
- [x] 2.5 `MainLayout` `CloseSidebarIfMobile()`：移除 JS eval，改判 `Viewport.IsMobile`
- [x] 2.6 `MainLayout` 實作 `Dispose()`：`Viewport.OnChanged -= HandleViewportChanged`

## 3. 文件

- [x] 3.1 在 repo 根目錄新建 `CONTEXT.md`，登錄 "Viewport breakpoint" 詞彙條目（引用 capability `viewport-breakpoint`）

## 4. SIT 驗證

- [x] 4.1 `/launch start`
- [x] 4.2 Chrome MCP 巡訪 `/`，確認 sidebar 預設行為符合（>=768 開、<768 關）
- [x] 4.3 **resize bug 驗證**：拖視窗從桌機寬度到 mobile 寬度，sidebar 應自動關
- [x] 4.4 **resize bug 驗證反向**：拖視窗從 mobile 回桌機寬度，sidebar 應自動開
- [x] 4.5 手機寬度點任一導覽連結，sidebar 應收起（保留現有行為）
- [x] 4.6 巡訪 `/counter`、`/weather`、`/playground`，確認無 regression、console 無 unhandled exception
- [x] 4.7 Dev tools Network panel 確認 `viewport-breakpoint.js` 載入成功（200，無 404）
- [x] 4.8 `/launch stop`

## 5. 提交

- [x] 5.1 以單一 commit 提交所有實作 + OpenSpec artifacts，commit message 採用 `Refactor:` 或 `Implement:` 前綴並說明 viewport seam 抽出 + resize bug 修復
