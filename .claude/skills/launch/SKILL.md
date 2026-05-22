---
name: launch
description: BlazorN10WasmLab 完整啟動／停止流程。啟動（start）：以 .NET Aspire 啟動專案並開啟瀏覽器；停止（stop）：正常終止 Aspire 程序。使用方式：/launch start 或 /launch stop。
trigger: /launch
---

# /launch

BlazorN10WasmLab 啟動流程管理。以 args 決定動作：

| 指令 | 說明 |
|---|---|
| `/launch start` | 啟動 Aspire、開啟瀏覽器、導覽至首頁 |
| `/launch stop` | 正常終止 Aspire 程序 |
| `/launch`（無 args）| 自動偵測：未啟動則 start，已啟動則詢問 |

---

## /launch start

### Step 1 — 檢查是否已在執行

讀取 `.claude/aspire-debug_session.tmp`（若存在），確認上一次的 task_id。
若檔案存在，告知使用者「Aspire 可能已在執行（task: {task_id}）」並詢問是否重新啟動。
若使用者確認重新啟動，先執行 stop 流程（見下方）再繼續。

### Step 2 — 在背景啟動 Aspire

先確認 log 目錄存在（Bash tool）：

```bash
mkdir -p .claude/tmp
```

以 Bash tool（`run_in_background: true`）執行，使用 `tee` 同時寫入本地 log：

```bash
dotnet run --project BlazorN10WasmLab.AppHost 2>&1 | tee .claude/tmp/aspire-startup.log
```

取得背景任務的 task_id，立即將其寫入暫存檔 `.claude/aspire-debug_session.tmp`（純文字，一行）。

### Step 3 — 等待啟動完成

使用 **Bash tool** 輪詢本地 log 檔（每 3 秒一次，最多 60 秒）：

```bash
elapsed=0
while [ $elapsed -lt 60 ]; do
    grep -q "Distributed application started" .claude/tmp/aspire-startup.log 2>/dev/null && break
    sleep 3
    elapsed=$((elapsed + 3))
done
if [ $elapsed -ge 60 ]; then
    echo "❌ 啟動逾時"; tail -20 .claude/tmp/aspire-startup.log; exit 1
fi
tail -5 .claude/tmp/aspire-startup.log
```

從輸出中解析：
- `Now listening on: https://localhost:XXXXX` → Dashboard URL
- `Login to the dashboard at https://localhost:XXXXX/login?t=...` → 登入連結

顯示給使用者：

```
✅ Aspire 已啟動
   Dashboard : https://localhost:XXXXX
   登入連結  : https://localhost:XXXXX/login?t=...
```

### Step 4 — 開啟 Chrome 並導覽至應用程式

使用 `mcp__claude-in-chrome__tabs_context_mcp`（`createIfEmpty: true`）取得或建立 tab，
再以 `mcp__claude-in-chrome__navigate` 導覽至：

```
https://blazorn10wasmlab.dev.localhost:7009/
```

等待頁面 title 變為 "Home"。

### Step 5 — 截圖確認

使用 `mcp__claude-in-chrome__computer`（`action: screenshot`）截圖，
確認左側導覽列顯示 **Home / Counter / Weather**。

回報結果：

```
✅ 應用程式已就緒
   URL: https://blazorn10wasmlab.dev.localhost:7009/
   導覽列：Home / Counter / Weather ✓
```

---

## /launch stop

### Step 1 — 讀取執行中的 task_id

讀取 `.claude/aspire-debug_session.tmp`，取得 task_id。

若檔案不存在，回報「找不到執行中的工作階段，請確認 Aspire 是否已啟動。」並停止。

### Step 2 — 正常終止 Aspire

使用 `TaskStop` tool，傳入讀取到的 task_id：

```
TaskStop(task_id: "{task_id}")
```

### Step 3 — 清除暫存檔

刪除 `.claude/aspire-debug_session.tmp`：

```powershell
Remove-Item -Force .claude\aspire-debug_session.tmp
```

### Step 4 — 回報結果

```
✅ 工作階段已結束
   task_id : {task_id}
   Aspire 程序已正常終止
```

---

## 自動偵測模式（無 args）

1. 檢查 `.claude/aspire-debug_session.tmp` 是否存在
2. **存在** → 顯示目前狀態，詢問使用者「要停止工作階段嗎？」
   - 確認 → 執行 stop 流程
   - 取消 → 顯示 Dashboard URL 後結束
3. **不存在** → 直接執行 start 流程
