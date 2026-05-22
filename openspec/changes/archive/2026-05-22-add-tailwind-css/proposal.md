## Why

專案目前使用 Bootstrap 作為 CSS 框架，但 Bootstrap 的 component-based 類別系統與 Tailwind 的 utility-first 方法並存會產生命名衝突，且長期維護兩套 class 系統成本高。改用 Tailwind CSS v4 可統一 styling 策略，並透過 standalone CLI 不引入 Node.js 建置依賴。

## What Changes

- 移除 Bootstrap（從 `wwwroot/lib/bootstrap/` 目錄與 `App.razor` 的 `<link>` 標籤）
- 新增 Tailwind CSS v4 standalone CLI（放在 solution root `.tools/tailwindcss.exe`，加入 `.gitignore`）
- 新增 Tailwind input CSS（`BlazorN10WasmLab/Styles/app.css`），含 `@import "tailwindcss"` 與 `@source` 掃描 `.razor` 檔案路徑設定
- 新增 MSBuild target 在 `dotnet build` 時自動執行 Tailwind CLI，輸出到 `wwwroot/app.css`
- 重寫 `NavMenu.razor`、`MainLayout.razor` 的 Bootstrap class 為 Tailwind utilities
- 原 `app.css` 的 Blazor 特定樣式（`.blazor-error-boundary` 等）遷移至 Tailwind input CSS

## Capabilities

### New Capabilities

- `tailwind-build-pipeline`: 透過 Tailwind CSS v4 standalone CLI 與 MSBuild 整合，在 `dotnet build` 時自動產生最小化 CSS，掃描 `.razor` 檔案中實際使用的 utility class

### Modified Capabilities

（無）

## Impact

- **移除依賴**：Bootstrap 5（`wwwroot/lib/bootstrap/`）
- **新增外部工具**：`tailwindcss.exe`（standalone CLI，不納入 git）
- **修改檔案**：`BlazorN10WasmLab.csproj`、`App.razor`、`NavMenu.razor`、`MainLayout.razor`、`NavMenu.razor.css`、`MainLayout.razor.css`、`app.css`
- **新增檔案**：`BlazorN10WasmLab/Styles/app.css`（Tailwind input）、`.tools/.gitkeep`
