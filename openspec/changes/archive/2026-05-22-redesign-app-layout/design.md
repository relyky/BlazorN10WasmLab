## Context

BlazorN10WasmLab 使用 Blazor WASM + .NET Aspire，CSS 框架為 Tailwind CSS v4（standalone CLI，無 npm）。現有 Layout 沿用 Blazor 預設模板，視覺結構為：深色漸層側邊欄 + 頂端連結列 + 白色內容區。  
本設計需在不引入新套件、不破壞現有 gRPC 功能的前提下，改為企業入口風格的三層 Layout。

## Goals / Non-Goals

**Goals:**
- 定義品牌色系並整合至 Tailwind v4 `@theme`，讓 `.razor` 中可直接用 utility class
- 改為頂部固定 Header + 左側固定 Sidebar + 右側 main 滾動的三層 flexbox 骨架
- 側邊欄支援可折疊群組（純 Blazor 狀態，無 JavaScript）
- 首頁改為儀表板樣式：靜態公告 Banner + 功能入口卡片 Grid

**Non-Goals:**
- 響應式漢堡選單（mobile breakpoint sidebar collapse）
- 動態公告資料（不連接後端）
- 用戶登入/登出流程、頭像顯示
- 深色模式

## Decisions

### 1. 品牌色整合方式：Tailwind v4 `@theme`

**選擇**：在 `BlazorN10WasmLab/Styles/app.css` 的 `@import "tailwindcss"` 後加入：

```css
@theme {
  --color-primary: #c2185b;
  --color-primary-dark: #880e4f;
  --color-primary-light: #f06292;
  --color-primary-fg: #ffffff;
}
```

**理由**：Tailwind v4 的設計哲學是 CSS-first，`@theme` 讓自訂色系直接成為 utility class（`bg-primary`、`text-primary` 等），比 v3 的 `tailwind.config.js` 更簡潔，且符合現有 standalone CLI 建置流程。

**替代方案放棄**：直接用 hardcode hex（`bg-[#c2185b]`）— 散落各處難以維護，變更時需全域搜尋替換。

### 2. Layout 骨架：移除 `.page`/`.sidebar` class 依賴，改為 Tailwind

**選擇**：`MainLayout.razor` 完全用 Tailwind utility 組合結構，廢棄 `MainLayout.razor.css` 中的 `.page`、`.sidebar`、`.top-row` 定義。

```
<div class="flex flex-col h-screen">
  <header class="shrink-0 bg-primary text-primary-fg ...">  ← 頂部 Header
  <div class="flex flex-1 overflow-hidden">
    <nav class="w-60 shrink-0 bg-white border-r ...">       ← 側邊欄
    <main class="flex-1 overflow-y-auto bg-gray-50 p-6">   ← 主內容
```

**理由**：scoped CSS 保留只做 Tailwind utility 無法簡潔表達的部分（`#blazor-error-ui` 固定定位）。

### 3. 可折疊側邊欄：Blazor `@code` bool toggle

**選擇**：`NavMenu.razor` 用 `private bool _showGroup = true;` + `@onclick="() => _showGroup = !_showGroup"` 控制展開/收合。

**理由**：MVP 最簡，無需 CSS hack（checkbox trick）或 JS 互操作。Blazor WASM re-render 成本極低。

**替代方案放棄**：HTML `<details>/<summary>` — 動畫及樣式控制較不靈活；CSS checkbox hack — 可讀性差。

### 4. App 卡片 Grid：Home.razor inline，不建新元件

**選擇**：卡片 HTML 直接寫在 `Home.razor`，用 Tailwind Grid。

**理由**：目前只有 2 張卡片（Counter、Weather），MVP 不做過早抽象。若未來卡片超過 4 張或需動態資料再提取為元件。

## Risks / Trade-offs

- **`MainLayout.razor.css` 殘留樣式衝突** → 清空舊的 `.page`、`.sidebar`、`.top-row` 定義，只保留 `#blazor-error-ui` 相關 CSS
- **`NavMenu.razor.css` 的 `::deep` selector** → 舊的深色主題 `.nav-item ::deep .nav-link` 顏色定義需移除，改用 Tailwind utility 直接上色
- **Tailwind `@theme` 自訂色需重新 build** → `dotnet build` 時 MSBuild target 會自動重跑 Tailwind CLI，開發時需注意 hot reload 可能需要手動 rebuild
