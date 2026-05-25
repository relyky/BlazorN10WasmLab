# CONTEXT

本檔案累積專案的領域語言（domain vocabulary）。新術語在被引入時即時登錄，避免散落於 commit message 與註解。

## 詞彙

- **Viewport breakpoint** — 視窗寬度跨越 Tailwind `md:`（768px）斷點的事件來源。由 `IViewportBreakpoint` 提供，封裝 `window.matchMedia` 互動，使元件能以「目前是不是 mobile」單一概念回應 layout 變化。對應 capability `viewport-breakpoint`（`openspec/specs/viewport-breakpoint/spec.md`）。
