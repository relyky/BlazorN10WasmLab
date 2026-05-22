## Why

目前側邊欄為固定寬度，在手機螢幕上佔用過多空間，且無法收合，影響行動裝置的使用體驗。需要讓使用者能透過點擊 AppBar Logo 來切換側邊欄的展開/收合狀態，並針對不同螢幕寬度提供合適的行為。

## What Changes

- AppBar 的 Logo 圓圈改為可點擊按鈕，點擊觸發側邊欄 toggle
- 手機（< 768px）：側邊欄以 Overlay 抽屜模式呈現（固定定位、半透明遮罩），預設收合
- 桌面（≥ 768px）：側邊欄以 inline 模式呈現（佔據 flex 空間），預設展開，但可 toggle 收合
- 頁面初次載入時以 JavaScript interop 偵測螢幕寬度，決定初始展開/收合狀態
- 手機模式下，點擊導覽項目後自動關閉側邊欄
- 側邊欄展開/收合加入 CSS `transition-transform` 滑入/出動畫

## Capabilities

### New Capabilities

- `sidebar-toggle`：側邊欄響應式折疊控制 — 含 Logo 點擊 toggle、xs Overlay 模式、md+ inline 模式、動畫、自動關閉

### Modified Capabilities

- `app-shell`：Header 的 Logo 由靜態展示改為可點擊的 toggle 按鈕（行為需求變更）

## Impact

- `BlazorN10WasmLab.Client/Layout/MainLayout.razor` — 加入 toggle 狀態管理、JS interop、遮罩、側邊欄樣式條件切換
- `BlazorN10WasmLab.Client/Layout/NavMenu.razor` — 加入 `OnNavigated` EventCallback
- `BlazorN10WasmLab.Client/Layout/MainLayout.razor.css` — 無需改動
- 無新套件依賴、無 gRPC 變更、無 API 變更
- 需要 `IJSRuntime` 注入（Blazor 內建，無需額外套件）
