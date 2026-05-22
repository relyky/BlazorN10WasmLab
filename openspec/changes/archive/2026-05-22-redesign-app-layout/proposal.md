## Why

目前的 Layout 使用 Blazor 預設骨架，缺乏品牌識別（無頂部 Header）、側邊欄視覺層次不清（扁平無群組），首頁只有 "Hello, world!" 佔位文字。需要對標企業入口網站的標準 Layout 模式：固定頂部品牌列 + 可折疊分組側邊欄 + 儀表板式首頁。

## What Changes

- 新增全寬固定頂部 Header Bar，包含品牌 Logo、應用名稱、用戶資訊區
- 側邊欄改為可折疊群組結構（群組標頭可展開/收合）
- 定義 Pink 品牌色系（`#c2185b`）至 Tailwind v4 `@theme`
- 首頁（Home.razor）改為儀表板樣式：靜態公告 Banner + App 卡片 Grid（每張卡片對應一個功能頁面）
- 清理並精簡 scoped CSS（移除舊的 `.top-row`、`.navbar-toggler`、深色側邊欄漸層等殘留樣式）

## Capabilities

### New Capabilities

- `app-shell`：整體頁面骨架 — 頂部 Header + 側邊欄 + 主內容區的三層 flexbox 結構，含品牌色主題定義
- `home-dashboard`：首頁儀表板內容 — 靜態公告 Banner + 功能頁面 App 卡片 Grid

### Modified Capabilities

（無，現有頁面功能需求不變，只改視覺結構）

## Impact

- `BlazorN10WasmLab.Client/Layout/MainLayout.razor` — 完全改寫
- `BlazorN10WasmLab.Client/Layout/MainLayout.razor.css` — 大幅精簡
- `BlazorN10WasmLab.Client/Layout/NavMenu.razor` — 完全改寫
- `BlazorN10WasmLab.Client/Layout/NavMenu.razor.css` — 大幅精簡
- `BlazorN10WasmLab.Client/Pages/Home.razor` — 完全改寫
- `BlazorN10WasmLab/Styles/app.css` — 加入 `@theme` 品牌色變數
- 無 API 變更、無新套件依賴、無 gRPC 變更
