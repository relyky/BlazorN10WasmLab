## ADDED Requirements

### Requirement: 全站採用 WebAssembly 互動 render mode 且關閉 prerender
應用程式 SHALL 在 `App.razor` 將 `<HeadOutlet>` 與 `<Routes>` 的 render mode 統一設定為 `InteractiveWebAssemblyRenderMode(prerender: false)`，使所有頁面僅在瀏覽器 WASM 啟動後渲染，server 端不執行預渲染。

#### Scenario: HeadOutlet 與 Routes 使用同一個 render mode 實例
- **WHEN** 開發者檢視 `App.razor`
- **THEN** `<HeadOutlet>` 與 `<Routes>` 皆透過同一個 `static readonly InteractiveWebAssemblyRenderMode` 欄位（命名 `WasmNoPrerender`）綁定 render mode，避免重複實例化與「改一處忘改另一處」

#### Scenario: 任何頁面在 server 端不執行預渲染
- **WHEN** server 收到任何路由的 HTTP 請求（例如 `/`、`/counter`、`/weather`、`/playground`）
- **THEN** server 回傳的 HTML SHALL 僅包含 WASM 載入殼層，不包含頁面元件樹的預渲染內容；頁面實際內容必須等 client WASM 啟動後才渲染出來

#### Scenario: 注入 client-only 服務的頁面可正常載入
- **WHEN** 使用者瀏覽 `/weather`（頁面內 `@inject IWeatherService`，該服務僅在 client DI 註冊）
- **THEN** server 不會嘗試解析 `IWeatherService`，頁面不拋出 `InvalidOperationException`；WASM 啟動後 client DI 正常注入，gRPC-Web 通訊正常運作

### Requirement: Client-only 服務不需要在 server DI 鏡像註冊
應用程式 SHALL 維持「只在 client `Program.cs` 註冊 client-only DI 服務（例如 gRPC client 包裝類別）」的設計邊界，server `Program.cs` 不重複註冊這些服務。

#### Scenario: 新增 client-only 服務時不需動 server 端 DI
- **WHEN** 開發者新增一個只供 Blazor 頁面使用的 client-only 服務（例如新的 gRPC-Web client 包裝類別）
- **THEN** 該服務只需要在 `BlazorN10WasmLab.Client/Program.cs` 註冊，不需要在 server `BlazorN10WasmLab/Program.cs` 鏡像註冊；prerender 已關閉，server 不會試圖解析

### Requirement: Render mode 設定需附註解說明關閉 prerender 的原因
`App.razor` 中宣告 `WasmNoPrerender` 欄位的位置 SHALL 在欄位上方加一行繁體中文註解，說明關閉 prerender 是為了避免 server 解析只在 client DI 註冊的服務。

#### Scenario: 註解明確點出 DI 邊界
- **WHEN** 開發者檢視 `App.razor` 的 `@code` 區塊
- **THEN** `WasmNoPrerender` 欄位上方 SHALL 存在一行繁中註解，內容明確提及「避免 server 解析只在 client DI 註冊的服務」的設計意圖，使後續維護者不會誤改回 `prerender: true`
