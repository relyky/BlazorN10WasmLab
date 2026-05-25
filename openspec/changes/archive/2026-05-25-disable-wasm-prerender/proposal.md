## Why

`/weather` 頁面載入時拋出 `InvalidOperationException`，因為 server prerender 試圖解析 `@inject IWeatherService` 但該服務只註冊在 Client DI。本專案是純 WASM 互動架構、無 SEO 需求，prerender 帶來的首屏優勢遠小於它強迫「server 必須鏡像 client DI」的維護負擔。

## What Changes

- 全站 `<HeadOutlet>` 與 `<Routes>` 的 render mode 由預設 `InteractiveWebAssembly`（prerender=true）改為 `new InteractiveWebAssemblyRenderMode(prerender: false)`
- App.razor `@code` 區塊新增 `private static readonly InteractiveWebAssemblyRenderMode WasmNoPrerender` 欄位供兩處共用
- 欄位上方加一行繁體中文註解說明關閉 prerender 的原因（避免未來誤改）

## Capabilities

### New Capabilities
- `wasm-render-mode`: 規範整個應用以 WebAssembly 為唯一互動 render mode，並關閉 server prerender；同時定義「client-only DI 服務不需要在 server 端註冊」的設計邊界。

### Modified Capabilities
（無 — 既有 capability 皆不涉及 render mode 設定）

## Impact

- **修改檔案**：`BlazorN10WasmLab/BlazorN10WasmLab/Components/App.razor`（單一檔案、2 行屬性 + 1 段 `@code`）
- **行為變更**：
  - `/weather` 不再拋出 prerender 例外（修復 bug）
  - 所有頁面首屏會多一個 WASM boot 空窗（純 lab 專案可接受）
  - 失去 server 預先產出 HTML 的 SEO/首屏優勢（本專案無此需求）
- **不影響**：gRPC-Web 通訊、Tailwind 編譯、Aspire orchestrator、現有 `MainLayout` 與 `IJSRuntime` 互動行為（已驗證 JS interop 寫在 `OnAfterRenderAsync(firstRender)`，不依賴 prerender）
- **未來開發者**：新增的頁面若注入 client-only service，不再需要在 server DI 鏡像註冊
