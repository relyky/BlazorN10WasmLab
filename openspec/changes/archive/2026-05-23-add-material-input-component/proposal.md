## Why

專案目前沒有任何可重用 UI 元件，所有 Razor 元件直接放在 `Pages/` 或 `Layout/`。當未來頁面需要表單輸入時，會各自重複實作 input 樣式。先建立第一個共享元件（Material 風格 floating-label input）並確立 `Client/Shared/` 落點，為後續共用元件奠基。同時需要一個 sandbox 頁面讓共用元件能被視覺驗證，不干擾既有的 Counter / Weather 展示。

## What Changes

- 新增共享元件目錄 `BlazorN10WasmLab.Client/Shared/`，並把 `BlazorN10WasmLab.Client.Shared` namespace 加入 `_Imports.razor`
- 新增 `MaterialInput.razor`：Material 風格浮動標籤 input，雙向繫結 `string?`、`oninput` 即時更新、未整合 `EditContext`（限非 EditForm 場景）
- 主題色採用專案既有 `var(--color-primary)`（取代參考程式硬寫的 `indigo-600`）
- `Id` 參數設為 nullable，未提供時在 `OnInitialized` 內生成 stable id（避免每 render 換值）
- 新增 `Playground.razor` 頁面（`/playground`），作為共享元件視覺驗證 sandbox
- `NavMenu.razor` 加入 Playground 連結

## Capabilities

### New Capabilities

- `shared-ui-components`: 共享 UI 元件模組（資料夾結構、命名空間、首批元件 MaterialInput 規格）
- `playground-page`: 共享元件展示與手動驗證的 sandbox 頁面（路由、佈局、demo 內容）

### Modified Capabilities

- `app-shell`: NavMenu 新增 Playground 入口連結（屬於 navigation requirement 變更）

## Impact

- **新增檔案**：
  - `BlazorN10WasmLab.Client/Shared/MaterialInput.razor`
  - `BlazorN10WasmLab.Client/Pages/Playground.razor`
- **修改檔案**：
  - `BlazorN10WasmLab.Client/_Imports.razor`（加 `@using ...Shared`）
  - `BlazorN10WasmLab.Client/Layout/NavMenu.razor`（加 Playground NavLink）
- **不影響**：Server、`Program.cs`、Routes.razor、Tailwind 設定（CSS 透過 watch 自動再生）
- **依賴**：純前端 Blazor 元件，無新增 NuGet 套件
- **風險**：`oninput` 與中文 IME 組字過程會即時觸發 `ValueChanged`，MVP 階段接受此行為
