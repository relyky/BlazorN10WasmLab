## Context

`App.razor` 目前對 `<HeadOutlet>` 與 `<Routes>` 都套用 `@rendermode="InteractiveWebAssembly"`，這是 `RenderMode.InteractiveWebAssembly` 靜態實例的快捷寫法，等同 `prerender: true`。Server 在處理請求時會先以 SSR 預渲染整個元件樹，再讓 client WASM 接手 hydrate。

預渲染 Weather 頁時，框架在 `SetParametersAsync` 階段嘗試從 server DI 解析 `IWeatherService`，但 `IWeatherService` 只在 `BlazorN10WasmLab.Client/Program.cs:18` 註冊，server `Program.cs` 沒有對應註冊（且依設計也不該有 — server 端只透過 `MapGrpcService<WeatherService>` 把實作當 gRPC handler 暴露，並非作為 page-injectable service）。

專案內 grep 確認：`@rendermode` 只在 `App.razor` 出現兩次，沒有 per-component override；其他頁面（Home、Counter、Playground、NotFound）均無 `@inject`，`MainLayout` 注入的 `IJSRuntime` 兩端都有，JS interop 寫在 `OnAfterRenderAsync(firstRender)` 不依賴 prerender。

## Goals / Non-Goals

**Goals:**
- 消除 `/weather` 的 prerender DI 例外
- 建立「client-only DI 服務不需要 server 鏡像註冊」的設計邊界，避免未來新頁面踩同樣的雷
- 改動範圍最小（單檔、3 處變更）

**Non-Goals:**
- 不修改 `IWeatherService` 註冊方式（server 維持只當 gRPC handler）
- 不重構 Weather.razor 的 `@inject` 或 lifecycle 寫法
- 不調整 SEO、首屏 HTML 優化（本專案無此需求）
- 不導入 prerender 開關的 runtime 設定機制（YAGNI）

## Decisions

### D1：整站 prerender=false，而非單頁 override

**選項：**
- A. App.razor 兩處全改 `prerender: false`（整站）
- B. 只在 Weather.razor 加 `@attribute [RenderModeAttribute(...)]` per-page override
- C. 拆 Weather 為 shell + child component，子元件單獨關 prerender

**選擇：A**

**理由：**
- 本專案是純 WASM 互動架構，無 server-interactive 頁面，prerender 對其他頁面也只是邊際優勢
- B/C 把問題收得很窄，但每次新增「需要 client DI」的頁面都得重複套用，違反 DRY
- A 一次解決現在與未來，且 App.razor 是顯眼入口檔案、誤改機率低

### D2：抽 static field `WasmNoPrerender`，而非兩處 inline `new`

**選項：**
- A. 兩處 inline `new InteractiveWebAssemblyRenderMode(prerender: false)`
- B. App.razor `@code` 區塊宣告 `private static readonly` 欄位共用
- C. 獨立 `RenderModes.cs` 靜態類別，跨檔案共用

**選擇：B**

**理由：**
- 兩處使用相同實例，抽欄位避免「改一處忘改另一處」
- C 的跨檔案重用沒有實際場景（只有一個 `App.razor`），增加搜尋成本而無收益
- B 在同檔案內視覺距離近，命名 `WasmNoPrerender` 清楚說明意圖

### D3：加一行繁中註解說明 why

**選項：**
- A. 不加註解，依賴 git blame + commit message
- B. 在 static field 上方加一行說明「為什麼關 prerender」

**選擇：B**

**理由：**
- 專案 `CLAUDE.md` 規定預設不寫註解，但允許「非顯而易見的 why」例外 — 此情境符合
- App.razor 是新加入者最早接觸的檔案之一，被誤改成 `prerender: true` 的風險不低
- 註解內容（DI 邊界決策）長期穩定，不會 rot

## Risks / Trade-offs

- **首屏空窗**：所有頁面在 WASM boot 完成前都會見到空白 → 純 lab 專案可接受；Counter 已實測載入後渲染流暢
- **失去 SEO HTML**：搜尋引擎只看到空 shell → 本專案無 SEO 需求
- **未來若要恢復 prerender**：需同時在 server `Program.cs` 鏡像註冊所有 client-only DI 服務；這個成本在當下不必付，但要意識到「關 prerender」是個雙向門，恢復時需配套
