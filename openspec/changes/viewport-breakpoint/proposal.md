## Why

`MainLayout.razor` 內兩處重複用 `JS.InvokeAsync<int>("eval", "window.innerWidth")` 配 magic number `768` 判斷 mobile，邏輯散落且沒模型化。更嚴重的是：完全沒監聽 resize，使用者把桌機視窗縮小到 mobile 寬度後 sidebar 仍占滿畫面（latent UX bug）。本變更把「viewport breakpoint」概念抽成可注入服務 `IViewportBreakpoint`，以 `window.matchMedia` 為基礎提供 push-based 通知，順手修掉 resize bug，並為未來其他需要 mobile detection 的元件留下乾淨 seam。

## What Changes

- 新增 `IViewportBreakpoint` 介面：`bool IsMobile`、`event Action OnChanged`、`ValueTask EnsureInitializedAsync()`
- 新增實作 `ViewportBreakpoint : IViewportBreakpoint, IAsyncDisposable`，封裝 `window.matchMedia` JS interop
- 新增 JS module `viewport-breakpoint.js`（init / dispose 對稱 API，dumb adapter，media query 字串由 C# 傳入）
- DI 註冊為 Singleton（client 端）
- `MainLayout` 移除兩處 `eval('window.innerWidth')`，改注入 `IViewportBreakpoint`、訂閱 `OnChanged`、實作 `IDisposable`
- **行為變更**：跨越斷點時 sidebar 自動同步（mobile → 關、desktop → 開），修復原本的 resize 不同步 bug
- 新增 `CONTEXT.md`（repo 根目錄），登錄 "viewport breakpoint" 術語

## Capabilities

### New Capabilities
- `viewport-breakpoint`：定義 client 端「視窗是否跨越 Tailwind `md:` 斷點」的事件來源與消費契約，封裝 `window.matchMedia` 互動。

### Modified Capabilities
（無）

## Impact

- **修改檔案**：`BlazorN10WasmLab.Client/Program.cs`（+1 行 DI）、`BlazorN10WasmLab.Client/Layout/MainLayout.razor`（移除 JS eval、改注入 + 訂閱）
- **新增檔案**：`Services/IViewportBreakpoint.cs`、`Services/ViewportBreakpoint.cs`、`wwwroot/js/viewport-breakpoint.js`、`CONTEXT.md`
- **行為改變**：sidebar 在 resize 跨越斷點時會自動開合（這是 UX 改進，但仍是行為變更，需 SIT 驗證）
- **不影響**：gRPC-Web、Tailwind 編譯、Aspire、其他頁面互動
- **未來收益**：任何元件需要 mobile detection 直接注入 `IViewportBreakpoint`，不再重複 JS eval + magic number
