## Context

`MainLayout.razor` 的 `SidebarClass` 在開啟態與關閉態切換時：

- 桌機：關閉態加 `md:hidden`（`display: none`）。瀏覽器對 `display` 切換**不觸發 CSS transition**，所以 `transition-transform duration-200` 對桌機完全沒作用，sidebar 直接消失。
- 手機：靠 `translate-x-0` ↔ `-translate-x-full` 滑出滑入，配 fixed 浮層，動畫正常。

兩種模式語意不同卻共用一份 class 字串，桌機被「動畫」誤導，實際是即時跳變。

另一個次要問題：`OnAfterRenderAsync` 第一次 render 才 JS eval `innerWidth` 設定 `_sidebarOpen`。若 transition 已掛上，hydrate 完成那一瞬間會看到一次非預期的 200ms 動畫（桌機從關閉跳到展開、手機反向）。

## Goals / Non-Goals

**Goals:**
- 桌機收合具備 200ms 視覺滑出 + main 同步展開的動畫
- 手機既有動畫不退化
- 首次 hydrate 不觸發動畫
- 純 Tailwind utility 解決，不新增 scoped CSS、不動 NavMenu

**Non-Goals:**
- 不改 sidebar 寬度（維持 `w-60`）
- 不調整 NavMenu 內部群組折疊動畫
- 不引入 JS 動畫庫
- 不改變現有的手機 overlay 行為與遮罩

## Decisions

### Decision 1: 用 `transform + negative margin` 而非 `width` 動畫

**選項：**
- A. `md:w-60` ↔ `md:w-0` + `overflow-hidden`：寬度動畫
- B. `translate-x` + `negative margin`：視覺滑出 + 佈局收回（**採用**）
- C. `grid-template-columns` 動畫：外層改 grid

**理由：**
- B 維持現有 flex 佈局，改動最小（只動 nav 元素自己）
- B 與手機既有的 transform 動畫一致，桌機與手機共用 `translate-x-0` ↔ `-translate-x-full` 字面
- A 會壓縮 sidebar 內容，視覺上 NavMenu 文字會被擠變形
- C 要改外層 `<div class="flex flex-1 overflow-hidden">` 為 grid，牽動手機 fixed 浮層疊放邏輯

**手法：**
```
開啟態：translate-x-0       md:ml-0
關閉態：-translate-x-full   md:-ml-60
```

`-translate-x-full` 把 sidebar 視覺推離視窗，`md:-ml-60`（-15rem，剛好等於 `w-60`）把它佔的佈局寬度抵銷，main 隨之展開。手機因 fixed 不受 margin 影響，只看 translate。

**重要：** Tailwind v4 的 `translate-x-*` 使用 CSS individual transform property `translate`（非 `transform`），所以 transition 屬性清單必須寫 `translate`，不能寫 `transform`，否則只會看到 margin 推動的錯覺、手機完全無動畫。

### Decision 2: 用 `transition-[translate,margin]` 而非 `transition-all`

**選項：**
- A. `transition-all`：所有屬性都 transition
- B. `transition-[translate,margin]`：明確列出兩個屬性（**採用**）

**理由：**
- B 精準，hover/active 等子元素互動不會被 200ms 拖慢
- A 在 sidebar 含有 hover 顏色變化的子元素時，會吃到非預期延遲

Tailwind v4 任意值 utility，JIT 從 markup 字串掃描，已涵蓋 `.razor`。

### Decision 3: 用 `_initialized` 旗標控制首次 transition

**做法：**
- 元件預設 `_initialized = false`
- 首次 render 之前，sidebar class 字串**不含** `transition-[translate,margin] duration-200`
- `OnAfterRenderAsync(firstRender: true)` 完成 `_sidebarOpen` 設定後，再設 `_initialized = true` 觸發第二次 render
- 第二次 render 起，class 字串才掛上 transition

如此初始狀態（無論桌機展開、手機收合）都是直接呈現，沒有 200ms 動畫。後續使用者點擊才走 transition。

**替代方案：用 CSS `prefers-reduced-motion` 或 JS 偵測 hydrate 完成**
否決理由：旗標方案最直接，狀態語意清楚。

### Decision 4: 共用 base class，差異只在 transform + margin

統一 nav 的所有 layout/外觀 class（`fixed top-16 bottom-0 left-0 z-40 w-60 bg-white border-r border-gray-200 overflow-y-auto shadow-lg md:static md:z-auto md:shadow-none md:shrink-0`），開關態差異**只剩兩組**：

| 狀態 | transform | md margin |
|---|---|---|
| 開啟 | `translate-x-0` | `md:ml-0` |
| 關閉 | `-translate-x-full` | `md:-ml-60` |

避免關閉態整段 class 換掉造成意外副作用（這就是原本 bug 的根源）。

## Risks / Trade-offs

- [手機收合時 `md:-ml-60` 不影響] → 因為手機是 fixed，margin 對 fixed 元素的佈局佔位無作用，純粹 transform 滑出即可，符合既有行為
- [`-ml-60` 與 `w-60` 數值必須一致] → 任一邊改寬度都要兩處同步；風險低（不常改），但可在 markup 留 inline 註解提示
- [Tailwind JIT 必須掃到 `transition-[translate,margin]`、`md:-ml-60`] → 已寫死在 markup 字面字串，`@source "../../BlazorN10WasmLab.Client/**/*.razor"` 涵蓋；可在 build 後檢查 `wwwroot/app.css` 確認 utility 已生成
- [`_initialized` 旗標多一次 render] → 影響微小，Blazor 元件本就會觸發多次 render
- [首次載入桌機從 SSR 預設值「閉」跳到「開」仍會閃一下] → 旗標只能保證沒有 200ms 動畫；SSR/CSR 狀態差異本身是視覺跳變，不在本次 scope（已在 sidebar-toggle 既有需求外）

## Migration Plan

純前端 layout 改動，無資料/API 變動，不需 migration。部署即生效。

回滾：還原 `MainLayout.razor` 即可。
