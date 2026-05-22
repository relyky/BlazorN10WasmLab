## Why

前一個 change `enable-launch-hot-reload` 啟用了 `dotnet watch` 的 Hot Reload，但實測發現：當 `.razor` 加入**新的** Tailwind utility class（如 `text-sky-400`、`text-[3em]`）時，畫面看不到變化。原因是 Tailwind CLI 綁在 MSBuild target，`dotnet watch` 的 Hot Reload 路徑跳過 MSBuild，因此 `wwwroot/app.css` 未被重新生成，新 utility 不存在於最終 CSS。目前的 workaround 是手動 `dotnet build` 或改用 inline `style`，兩者都打斷 Hot Reload 的快速迭代體驗。

## What Changes

- `/launch start` SKILL 額外啟動一個並行的背景程序：`.tools/tailwindcss.exe -i Styles/app.css -o wwwroot/app.css --watch`，與 `dotnet watch` 並行執行。
- `/launch stop` SKILL 同時終止 Tailwind watch 程序與 Aspire 程序。
- 新增 task_id 暫存機制以追蹤 Tailwind watch 程序（例如 `.claude/tailwind-watch_session.tmp`）。

## Capabilities

### New Capabilities
<!-- 無新 capability -->

### Modified Capabilities
- `launch-workflow`: 增加 Tailwind CLI watch 並行子程序的啟停要求。

## Impact

- **影響檔案**：`.claude/skills/launch/SKILL.md`（Step 2 + Step 3 啟動 / `/launch stop` Step 2 終止 / 暫存檔讀寫）
- **影響流程**：`/launch start` 完整生效後，編輯 `.razor` 加入新 Tailwind utility 數秒內即可看到瀏覽器套用。
- **新增 runtime 相依**：Tailwind CLI 已存在於 `.tools/tailwindcss.exe`，無新工具需求。
- **風險**：兩個 background task 同時管理，stop 流程需確保兩個都終止；任一啟動失敗時要清理另一個。
