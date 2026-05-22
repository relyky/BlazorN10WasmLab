## 1. 品牌色系定義

- [x] 1.1 在 `BlazorN10WasmLab/Styles/app.css` 的 `@import "tailwindcss"` 後加入 `@theme { --color-primary: #c2185b; --color-primary-dark: #880e4f; --color-primary-light: #f06292; --color-primary-fg: #ffffff; }`

## 2. MainLayout 重構

- [x] 2.1 改寫 `BlazorN10WasmLab.Client/Layout/MainLayout.razor`：移除 `.page`/`.sidebar`/`top-row` 結構，改為 `flex flex-col h-screen` 三層骨架（header + sidebar-main 區）
- [x] 2.2 清理 `BlazorN10WasmLab.Client/Layout/MainLayout.razor.css`：移除 `.page`、`.sidebar`、`.top-row` 定義，只保留 `#blazor-error-ui` 相關 CSS

## 3. NavMenu 重構

- [x] 3.1 改寫 `BlazorN10WasmLab.Client/Layout/NavMenu.razor`：移除 `navbar-toggler`/`nav-scrollable` 結構，改為含 `@code` bool toggle 的可折疊群組 + Tailwind utility 樣式
- [x] 3.2 清理 `BlazorN10WasmLab.Client/Layout/NavMenu.razor.css`：移除舊的深色主題 `.nav-link`、`.bi-*` 等已不需要的 class，保留必要的 `::deep` 選擇器（如有）

## 4. Home 儀表板頁面

- [x] 4.1 改寫 `BlazorN10WasmLab.Client/Pages/Home.razor`：加入靜態公告 Banner（含日曆圖示、主旨、說明文字）
- [x] 4.2 在 Home.razor 加入 App 卡片 Grid（2 欄，Counter 與 Weather 各一張卡片，含彩色圓形首字母 + 中英文名稱 + NavLink 導覽）

## 5. 建置與驗證

- [x] 5.1 執行 `dotnet build BlazorN10WasmLab.slnx` 確認 Tailwind 建置成功、無編譯錯誤
- [x] 5.2 啟動應用程式，確認 Header 顯示品牌色（`#c2185b`）且出現在所有頁面
- [x] 5.3 確認側邊欄群組可折疊（點擊「功能展示」toggle 展開/收合）
- [x] 5.4 確認首頁顯示公告 Banner + 兩張 App 卡片，且點擊卡片可導覽至對應頁面
- [x] 5.5 確認 Counter 與 Weather 頁面功能正常（計數器遞增、gRPC 資料載入）
