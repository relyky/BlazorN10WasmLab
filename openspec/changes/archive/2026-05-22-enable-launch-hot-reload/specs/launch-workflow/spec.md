## ADDED Requirements

### Requirement: Hot Reload 啟用於開發啟動

`/launch start` skill SHALL 以 `dotnet watch` 啟動 Aspire AppHost，使下游 Blazor Server 與 WebAssembly Client 在開發期間支援 .NET Hot Reload，修改 `.razor` / `.cs` / `.css` 不需重啟整個應用程式。

#### Scenario: 啟動後修改 Razor 元件即時生效
- **WHEN** 使用者執行 `/launch start` 並在瀏覽器開啟應用程式後，編輯 `BlazorN10WasmLab.Client/Pages/Counter.razor` 並存檔
- **THEN** Hot Reload SHALL 於數秒內套用變更至瀏覽器，無需重新啟動 Aspire

#### Scenario: 啟動指令使用 dotnet watch
- **WHEN** `/launch start` 進入背景啟動步驟
- **THEN** 實際執行的指令 MUST 以 `dotnet watch` 為基礎（而非 `dotnet run`），並指向 `BlazorN10WasmLab.AppHost` 專案

### Requirement: 背景模式下不卡在互動提示

啟動指令 SHALL 在非互動環境下執行，遇到 rude edit（不可熱更新的變更）時 MUST 自動 rebuild restart，禁止 `dotnet watch` 進入互動式提示而阻塞背景任務。

#### Scenario: Rude edit 觸發自動 restart
- **WHEN** 使用者修改了 Hot Reload 不支援的內容（如新增類別欄位）
- **THEN** `dotnet watch` MUST 自動執行 rebuild restart，不得 prompt 等待使用者輸入

### Requirement: 啟動完成偵測與既有流程相容

切換為 `dotnet watch` 後，SKILL 的啟動完成偵測（grep log 中 `Distributed application started`）、Dashboard URL 解析、Chrome 導覽、截圖確認等步驟 MUST 維持原行為，不得退化。

#### Scenario: 啟動成功訊息仍可偵測
- **WHEN** Aspire 透過 `dotnet watch` 成功啟動
- **THEN** `.claude/tmp/aspire-startup.log` MUST 在 60 秒內出現 `Distributed application started` 字串，後續導覽與截圖步驟 MUST 正常完成
