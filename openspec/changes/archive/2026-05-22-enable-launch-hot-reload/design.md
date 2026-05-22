## Context

`/launch start` skill 目前 Step 2 啟動指令為：

```bash
dotnet run --project BlazorN10WasmLab.AppHost 2>&1 | tee .claude/tmp/aspire-startup.log
```

`dotnet run` 不會啟動 Hot Reload watcher。Hot Reload 需要 `dotnet watch` 才會在檔案變更時透過 EnC（Edit and Continue）動態 patch 載入中的程式碼。Aspire AppHost SDK 13.1 已支援 `dotnet watch` 將 watch 行為傳播至下游被 `AddProject` 註冊的子專案（Server + WASM Client），因此只需替換 AppHost 端的啟動指令，即可獲得全鏈路 Hot Reload。

## Goals / Non-Goals

**Goals:**
- `/launch start` 啟動的 dev session 預設啟用 Hot Reload。
- 不破壞既有的 log 偵測、Dashboard URL 解析、Chrome 導覽流程。
- 背景模式下，rude edit 不得阻塞流程。

**Non-Goals:**
- 不調整 Aspire AppHost 程式碼或 launchSettings。
- 不引入 npm / 額外工具。
- 不處理 Tailwind CSS 的 watch（Tailwind 由 MSBuild target 在 build 時生成；Hot Reload 觸發 rebuild 時會一併重生 `wwwroot/app.css`，已足夠）。
- 不改變 `/launch stop` 行為。

## Decisions

### Decision 1：採用 `dotnet watch run` 而非 `dotnet watch`

兩者效果相同（`dotnet watch` 預設子命令為 `run`），但顯式寫出 `run` 較容易閱讀並避免未來 default 行為變動的風險。

最終指令：
```bash
dotnet watch --project BlazorN10WasmLab.AppHost --non-interactive run 2>&1 | tee .claude/tmp/aspire-startup.log
```

**為何 `--non-interactive`**：在背景任務（pipe to tee）下，`dotnet watch` 偵測到不可熱更新的變更時預設會印出 `watch : Do you want to restart your app - Yes (y) / No (n) / Always (a) / Never (v)?` 並等待輸入，這會讓背景 task hang。`--non-interactive` 強制 watch 不 prompt（行為相當於選擇預設 restart）。

**替代方案考慮**：
- 環境變數 `DOTNET_WATCH_RESTART_ON_RUDE_EDIT=1` — 已於 .NET 8 被 `--non-interactive` 旗標取代，後者更清晰。
- 改寫 SKILL 改用 `dotnet watch` 的 named pipe stdin — 過度設計，無必要。

### Decision 2：保留既有 log 偵測字串

`dotnet watch` 啟動子程序後，`Distributed application started` 仍會由 Aspire 印出。grep 條件不需更動。

唯一差異：watch 啟動時會多印 `dotnet watch 🚀 Started` 等訊息，但這些不影響 grep。

### Decision 3：不在 launchSettings 中新增 watch profile

維持「指令明示 watch」而非「launchSettings 配置 watch」，理由：
- launchSettings 影響 IDE F5 啟動行為，可能干擾使用者手動 debug 流程。
- SKILL 的 watch 行為應只在 `/launch start` 路徑生效，不污染其他啟動途徑。

## Risks / Trade-offs

- **[Risk] `dotnet watch` 啟動稍慢於 `dotnet run`（多了檔案監聽初始化）** → Mitigation: 啟動偵測 timeout 已是 60 秒，足夠涵蓋。
- **[Risk] WASM Hot Reload 對某些變更（新增 type、改變 method signature）仍會觸發完整 rebuild + 瀏覽器重新載入** → Mitigation: 此為 .NET WASM Hot Reload 本身限制，非本變更導入；rebuild restart 仍比手動 stop/start 快。
- **[Risk] `--non-interactive` 在 rude edit 時自動 restart 可能讓使用者誤以為 Hot Reload「失敗」** → Mitigation: log 中會明確印出 `Hot reload of changes failed, restarting...`，使用者可從 log 判斷。
- **[Risk] `tee` pipe 改變 stdout buffering，可能延遲 watch 訊息寫入 log** → Mitigation: `dotnet watch` 已使用 line-buffered output；現有流程已 work，watch 不會改變此行為。

## Migration Plan

1. 更新 SKILL.md Step 2 啟動指令。
2. 本地驗證：先 `/launch stop` 清掉殘留 task，再 `/launch start`，啟動後修改 `Counter.razor` 計數器初始值並存檔，瀏覽器無重整即看到變更則成功。
3. 無 rollback 機制需求：SKILL 變更為純文字修改，git revert 即還原。
