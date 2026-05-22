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

讀取 `.claude/aspire-debug_session.tmp` 與 `.claude/tailwind-watch_session.tmp`（若存在），確認上一次的 task_id。
若任一檔案存在，告知使用者「watch session 可能已在執行（aspire: {id1}, tailwind: {id2}）」並詢問是否重新啟動。
若使用者確認重新啟動，先執行 stop 流程（見下方）再繼續。

### Step 2 — 確認 Tailwind CLI 存在

先確認 log 目錄存在並檢查 standalone CLI：

```bash
mkdir -p .claude/tmp
test -f .tools/tailwindcss.exe || { echo "❌ 找不到 .tools/tailwindcss.exe — 請先下載 Tailwind CLI（見 CLAUDE.md）"; exit 1; }
```

若 CLI 不存在，明確報錯並停止流程，**不**啟動 Aspire。

### Step 3 — 啟動 Tailwind CLI watch（背景）

以 Bash tool（`run_in_background: true`）執行：

```bash
./.tools/tailwindcss.exe -i BlazorN10WasmLab/BlazorN10WasmLab/Styles/app.css -o BlazorN10WasmLab/BlazorN10WasmLab/wwwroot/app.css --watch=always > .claude/tmp/tailwind-watch.log 2>&1
```

> **為何並行 Tailwind watch**：`dotnet watch` 的 Hot Reload 路徑跳過 MSBuild，因此綁在 MSBuild target 上的 Tailwind CLI 不會重跑。新增 utility class 不會出現在 `wwwroot/app.css`。並行 watch 即時重生 CSS，補上這個落差。

取得 Tailwind 背景任務的 task_id，立即寫入暫存檔 `.claude/tailwind-watch_session.tmp`（純文字，一行）。

輪詢 Tailwind log，等待首次生成完成（最多 30 秒）：

```bash
elapsed=0
while [ $elapsed -lt 30 ]; do
    grep -q "Done in" .claude/tmp/tailwind-watch.log 2>/dev/null && break
    sleep 2
    elapsed=$((elapsed + 2))
done
if [ $elapsed -ge 30 ]; then
    echo "❌ Tailwind watch 啟動逾時"; tail -20 .claude/tmp/tailwind-watch.log; exit 1
fi
```

**失敗清理**：若 Tailwind 啟動逾時，MUST 對其 task_id 執行 `TaskStop` 並刪除暫存檔，**不**啟動 Aspire。

### Step 4 — 在背景啟動 Aspire

以 Bash tool（`run_in_background: true`）執行，使用 `dotnet watch` 啟用 Hot Reload：

```bash
dotnet watch --project BlazorN10WasmLab.AppHost --non-interactive run 2>&1 | tee .claude/tmp/aspire-startup.log
```

> **為何用 `dotnet watch`**：啟用 .NET Hot Reload，AppHost SDK 13.1 會將 watch 傳播至下游 Server 與 WASM Client，編輯 `.razor` / `.cs` 無需重啟整個應用。
> **為何加 `--non-interactive`**：背景任務（pipe to tee）下，遇到 rude edit（不可熱更新的變更）時 watch 預設會 prompt 等待輸入，會讓背景 task hang；`--non-interactive` 強制自動 rebuild restart，不 prompt。

取得背景任務的 task_id，立即將其寫入暫存檔 `.claude/aspire-debug_session.tmp`（純文字，一行）。

### Step 5 — 等待 Aspire 啟動完成

使用 **Bash tool** 輪詢本地 log 檔（每 3 秒一次，最多 60 秒）：

```bash
elapsed=0
while [ $elapsed -lt 60 ]; do
    grep -q "Distributed application started" .claude/tmp/aspire-startup.log 2>/dev/null && break
    sleep 3
    elapsed=$((elapsed + 3))
done
if [ $elapsed -ge 60 ]; then
    echo "❌ Aspire 啟動逾時"; tail -20 .claude/tmp/aspire-startup.log; exit 1
fi
tail -5 .claude/tmp/aspire-startup.log
```

**失敗清理**：若 Aspire 啟動逾時，MUST 對 Aspire task_id **與** Tailwind task_id 各執行 `TaskStop`，刪除兩份暫存檔，避免遺留孤兒 watch 程序。

從輸出中解析：
- `Now listening on: https://localhost:XXXXX` → Dashboard URL
- `Login to the dashboard at https://localhost:XXXXX/login?t=...` → 登入連結

顯示給使用者：

```
✅ watch session 已啟動
   Aspire    : task {aspire_id}, log: .claude/tmp/aspire-startup.log
   Tailwind  : task {tailwind_id}, log: .claude/tmp/tailwind-watch.log
   Dashboard : https://localhost:XXXXX
   登入連結  : https://localhost:XXXXX/login?t=...
```

### Step 6 — 開啟 Chrome 並導覽至應用程式

使用 `mcp__claude-in-chrome__tabs_context_mcp`（`createIfEmpty: true`）取得或建立 tab，
再以 `mcp__claude-in-chrome__navigate` 導覽至：

```
https://blazorn10wasmlab.dev.localhost:7009/
```

等待頁面 title 變為 "Home"。

### Step 7 — 截圖確認

使用 `mcp__claude-in-chrome__computer`（`action: screenshot`）截圖，
確認左側導覽列顯示 **首頁 / Counter / Weather**。

回報結果：

```
✅ 應用程式已就緒
   URL: https://blazorn10wasmlab.dev.localhost:7009/
   導覽列：首頁 / Counter / Weather ✓
```

---

## /launch stop

### Step 1 — 讀取執行中的 task_id

同時讀取兩份暫存檔：

- `.claude/aspire-debug_session.tmp` → `aspire_task_id`
- `.claude/tailwind-watch_session.tmp` → `tailwind_task_id`

若**兩份檔案皆不存在**，回報「找不到執行中的工作階段。」並停止。
若僅其中一份存在，仍繼續對存在的 task_id 處理，並在最終回報中標明另一邊「無 session」。

### Step 2 — 分別終止兩個 watch 程序

對每個取得的 task_id 各執行 `TaskStop`：

```
TaskStop(task_id: "{aspire_task_id}")
TaskStop(task_id: "{tailwind_task_id}")
```

任一 `TaskStop` 失敗時 log error 訊息，但 **MUST 繼續嘗試另一個**，不得提早中止流程。

### Step 3 — 清除暫存檔

刪除兩份暫存檔（不存在的略過）：

```bash
rm -f .claude/aspire-debug_session.tmp .claude/tailwind-watch_session.tmp
```

### Step 4 — 回報結果

```
✅ watch session 已結束
   Aspire   : task {aspire_task_id} — {ok|error}
   Tailwind : task {tailwind_task_id} — {ok|error}
   暫存檔已清除
```

---

## 自動偵測模式（無 args）

1. 檢查 `.claude/aspire-debug_session.tmp` 或 `.claude/tailwind-watch_session.tmp` 是否存在
2. **任一存在** → 顯示目前狀態，詢問使用者「要停止工作階段嗎？」
   - 確認 → 執行 stop 流程
   - 取消 → 顯示 Dashboard URL 後結束
3. **皆不存在** → 直接執行 start 流程
