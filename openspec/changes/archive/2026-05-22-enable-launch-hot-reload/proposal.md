## Why

目前 `/launch start` skill 以 `dotnet run --project BlazorN10WasmLab.AppHost` 啟動 Aspire，此指令**不啟用 .NET Hot Reload**，導致開發過程中修改 `.razor` / `.cs` / `.css` 都必須整個重啟才能看到變更。對 Blazor WASM Lab 這種以快速迭代驗證為主的專案，是明顯的開發體驗缺口。

## What Changes

- 將 `/launch start` 啟動指令從 `dotnet run` 改為 `dotnet watch run`，啟用 Hot Reload 至 AppHost 與其下游專案（Server + WASM Client）。
- 設定環境變數 `DOTNET_WATCH_RESTART_ON_RUDE_EDIT=1`（或等效旗標）讓 watch 在偵測到 unsupported edits 時自動 rebuild，避免背景模式下卡在互動提示。
- 更新 `.claude/skills/launch/SKILL.md` Step 2 啟動指令與相關註記。
- 不變動 Aspire AppHost 程式碼、launchSettings、或專案結構。

## Capabilities

### New Capabilities
- `launch-workflow`: 規範 `/launch` skill 的啟動／停止行為，包含 Hot Reload 啟用、背景執行、log 監聽與 Chrome 導覽。

### Modified Capabilities
<!-- 無既有 capability 受影響 -->

## Impact

- **影響檔案**：`.claude/skills/launch/SKILL.md`（修改 Step 2 啟動指令）
- **影響流程**：未來 `/launch start` 啟動後支援 Hot Reload；停止流程不變。
- **無 API、相依套件、或 runtime 變更。**
- **風險**：`dotnet watch` 在 rude edit 時可能 prompt；需以環境變數明確設定 non-interactive 行為，避免背景任務 hang。
