## 1. 環境準備

- [x] 1.1 在 solution root 建立 `.tools/` 目錄並新增 `.gitkeep`
- [x] 1.2 將 `.tools/` 加入 `.gitignore`
- [x] 1.3 在 `CLAUDE.md` 補充下載 tailwindcss standalone CLI 的指令說明
- [x] 1.4 下載 `tailwindcss-windows-x64.exe` 並重新命名為 `.tools/tailwindcss.exe`

## 2. Tailwind 建置設定

- [x] 2.1 建立 `BlazorN10WasmLab/Styles/app.css`，加入 `@import "tailwindcss"`、`@source` 指向兩個專案的 `.razor` 檔案
- [x] 2.2 將原 `wwwroot/app.css` 的 Blazor 特定樣式遷移至 `Styles/app.css`（`.blazor-error-boundary`、`.valid.modified` 等）
- [x] 2.3 在 `BlazorN10WasmLab.csproj` 加入 MSBuild target `TailwindBuild`（BeforeTargets="Build"），Debug 不加 `--minify`，Release 加 `--minify`
- [x] 2.4 在 MSBuild target 加入 `Condition` 檢查 `.tools/tailwindcss.exe` 是否存在，不存在時印出提示訊息並中止 build
- [x] 2.5 執行 `dotnet build` 確認 `wwwroot/app.css` 由 Tailwind 產生且包含預期 class

## 3. 移除 Bootstrap

- [x] 3.1 修改 `App.razor`，移除 Bootstrap `<link>` 標籤
- [x] 3.2 刪除 `wwwroot/lib/bootstrap/` 整個目錄

## 4. 元件樣式遷移

- [x] 4.1 重寫 `NavMenu.razor`，將 `navbar-*`、`nav-*`、`container-fluid` 等 Bootstrap class 換成 Tailwind utility class
- [x] 4.2 更新 `NavMenu.razor.css`，移除對 Bootstrap class 的依賴，保留 scoped 的 SVG icon 樣式與 `::deep` 選擇器
- [x] 4.3 重寫 `MainLayout.razor`，將 `px-4`（Bootstrap）換成 Tailwind 版本，確認 `.page` / `.sidebar` / `.content` 版面正常
- [x] 4.4 更新 `MainLayout.razor.css`，確認版面 CSS 不依賴 Bootstrap

## 5. 驗證

- [x] 5.1 執行 `dotnet run --project BlazorN10WasmLab.AppHost`，確認 app 正常啟動
- [x] 5.2 在瀏覽器確認 NavMenu、首頁、Counter、Weather 頁面版面正常
- [x] 5.3 確認 DevTools 中 `<head>` 不包含 Bootstrap `<link>`，且 `app.css` 由 Tailwind 產生
- [x] 5.4 執行 `dotnet build -c Release`，確認 `wwwroot/app.css` 為 minified 格式
