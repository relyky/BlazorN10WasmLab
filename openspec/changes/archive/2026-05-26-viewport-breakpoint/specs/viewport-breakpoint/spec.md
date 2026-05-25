## ADDED Requirements

### Requirement: 應用程式提供可注入的 viewport breakpoint 服務
應用程式 SHALL 在 client 端 DI 容器註冊 `IViewportBreakpoint` 服務（Singleton lifetime），讓任何元件可透過建構函式 / `@inject` 取得「目前視窗是否為 mobile 寬度」的即時值與變化通知。

#### Scenario: 元件可注入 IViewportBreakpoint
- **WHEN** 任何 Blazor client 元件以 `@inject IViewportBreakpoint Viewport` 宣告依賴
- **THEN** DI 容器 SHALL 解析出單一 `ViewportBreakpoint` 實例（同一 user circuit 內共用）

#### Scenario: 服務介面包含三個必要成員
- **WHEN** 開發者檢視 `IViewportBreakpoint` 介面
- **THEN** 介面 SHALL 包含：`bool IsMobile { get; }` 屬性、`event Action OnChanged` 事件、`ValueTask EnsureInitializedAsync()` 方法

### Requirement: Mobile 判定以 Tailwind md: 斷點為基準
`ViewportBreakpoint` 實作 SHALL 以 CSS media query `(max-width: 767.98px)` 為 mobile 判定基準，與 Tailwind `md:` 斷點（`>= 768px`）反向對齊。

#### Scenario: 視窗寬度 >= 768px 時 IsMobile 為 false
- **WHEN** 瀏覽器視窗寬度為 768px 以上且 `EnsureInitializedAsync()` 已完成
- **THEN** `IsMobile` SHALL 回傳 `false`

#### Scenario: 視窗寬度 < 768px 時 IsMobile 為 true
- **WHEN** 瀏覽器視窗寬度為 767.98px 以下且 `EnsureInitializedAsync()` 已完成
- **THEN** `IsMobile` SHALL 回傳 `true`

### Requirement: 跨越斷點時觸發 OnChanged 事件
`ViewportBreakpoint` SHALL 透過 `window.matchMedia` 監聽斷點變化，僅在視窗寬度「跨越」斷點時觸發 `OnChanged` 事件（不在每次 resize 觸發）。

#### Scenario: 從桌機寬度拖到 mobile 寬度
- **WHEN** 使用者把視窗從 >= 768px 寬度拖到 < 768px
- **THEN** `IsMobile` SHALL 變為 `true`，且 `OnChanged` 事件 SHALL 觸發一次

#### Scenario: 從 mobile 寬度拖到桌機寬度
- **WHEN** 使用者把視窗從 < 768px 拖到 >= 768px
- **THEN** `IsMobile` SHALL 變為 `false`，且 `OnChanged` 事件 SHALL 觸發一次

#### Scenario: 在同一斷點內 resize 不觸發事件
- **WHEN** 使用者在 >= 768px 範圍內 resize（例如 1200px → 1000px → 800px）
- **THEN** `OnChanged` 事件 SHALL 不觸發（matchMedia 天然 throttle）

### Requirement: 初始化採用 idempotent 顯式方法
`EnsureInitializedAsync()` SHALL 可被重複呼叫；首次呼叫執行 JS module import 與 matchMedia listener 註冊，後續呼叫 SHALL 為 no-op。在 `EnsureInitializedAsync()` 首次完成前，`IsMobile` SHALL 回傳預設值 `false`。

#### Scenario: 首次呼叫 EnsureInitializedAsync 完成 JS 端 setup
- **WHEN** 元件在 `OnAfterRenderAsync(firstRender: true)` 內呼叫 `await Viewport.EnsureInitializedAsync()`
- **THEN** 服務 SHALL 完成 JS module import、註冊 matchMedia listener、設置 `IsMobile` 為當前真實值

#### Scenario: 重複呼叫 EnsureInitializedAsync 為 no-op
- **WHEN** 任何元件第二次以後呼叫 `EnsureInitializedAsync()`
- **THEN** 服務 SHALL 立即回傳已完成的 `ValueTask`，不重複執行 JS interop

#### Scenario: 初始化前 IsMobile 為桌機預設
- **WHEN** `EnsureInitializedAsync()` 尚未被任何元件呼叫
- **THEN** `IsMobile` SHALL 回傳 `false`（桌機預設值）

### Requirement: 服務實作 IAsyncDisposable 釋放 JS listener
`ViewportBreakpoint` SHALL 實作 `IAsyncDisposable`，於 `DisposeAsync()` 呼叫 JS module 對稱的 dispose 函式移除 matchMedia listener。

#### Scenario: DisposeAsync 釋放 JS listener
- **WHEN** DI 容器在應用結束時釋放 `ViewportBreakpoint` 單例
- **THEN** `DisposeAsync()` SHALL 被呼叫，且 SHALL 透過 JS interop 呼叫 module 的 dispose 函式釋放 matchMedia listener

### Requirement: JS module 為純 dumb adapter
搭配的 JS module `viewport-breakpoint.js` SHALL 不寫死任何 media query 字串，所有斷點定義由 C# 端透過 `init(mediaQuery, dotNetRef)` 傳入。

#### Scenario: JS module 接受外部 media query 字串
- **WHEN** C# 呼叫 `init('(max-width: 767.98px)', dotNetRef)`
- **THEN** JS module SHALL 以該字串建立 `window.matchMedia` 並註冊 listener

#### Scenario: JS module 透過 DotNet callback 通知變化
- **WHEN** matchMedia `change` 事件觸發
- **THEN** JS module SHALL 呼叫 `dotNetRef.invokeMethodAsync('OnMatchChanged', mql.matches)` 將新值傳回 C#
