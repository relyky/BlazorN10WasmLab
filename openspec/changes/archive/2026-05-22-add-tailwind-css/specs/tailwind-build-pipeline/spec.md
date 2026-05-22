## ADDED Requirements

### Requirement: Tailwind CSS 在 dotnet build 時自動產生
系統 SHALL 在每次執行 `dotnet build` 時，自動呼叫 Tailwind CSS v4 standalone CLI，掃描所有 `.razor` 檔案中使用的 utility class，並將編譯後的 CSS 輸出至 `wwwroot/app.css`。

#### Scenario: 成功 build 時產生 CSS
- **WHEN** 執行 `dotnet build BlazorN10WasmLab.slnx`
- **THEN** `wwwroot/app.css` 被 Tailwind CLI 覆寫，內容包含所有 `.razor` 中使用的 Tailwind utility class

#### Scenario: Release build 產生 minified CSS
- **WHEN** 執行 `dotnet build -c Release`
- **THEN** `wwwroot/app.css` 為 minified 格式（移除空白與註解）

#### Scenario: standalone CLI 不存在時給出明確錯誤
- **WHEN** `.tools/tailwindcss.exe` 不存在時執行 `dotnet build`
- **THEN** build 失敗並輸出說明訊息，提示開發者需手動下載 CLI

### Requirement: Tailwind 掃描範圍涵蓋所有 Razor 元件
Tailwind CLI SHALL 掃描 server 專案與 client 專案的所有 `.razor` 檔案，確保任何元件中使用的 utility class 都被包含在 output CSS 中。

#### Scenario: Client 專案的 class 被包含
- **WHEN** `BlazorN10WasmLab.Client` 中的 `.razor` 檔案使用 `text-blue-500` 等 Tailwind class
- **THEN** output `wwwroot/app.css` 包含該 class 的對應 CSS 規則

#### Scenario: Server 專案的 class 被包含
- **WHEN** `BlazorN10WasmLab/Components` 中的 `.razor` 檔案使用 Tailwind class
- **THEN** output `wwwroot/app.css` 包含該 class 的對應 CSS 規則

### Requirement: Bootstrap 完全移除
系統 SHALL 移除 Bootstrap 相關資源，不再從 `wwwroot/lib/bootstrap/` 載入任何 CSS 或 JS。

#### Scenario: App.razor 不包含 Bootstrap link
- **WHEN** 應用程式啟動並載入頁面
- **THEN** HTML `<head>` 中不包含任何指向 Bootstrap 的 `<link>` 標籤

#### Scenario: Bootstrap 靜態檔案被移除
- **WHEN** 查看 `wwwroot/lib/` 目錄
- **THEN** `bootstrap/` 子目錄不存在
