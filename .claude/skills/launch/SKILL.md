---
name: launch
description: BlazorN10WasmLab 完整啟動／停止流程。啟動（start）：以 .NET Aspire 啟動專案並開啟瀏覽器；停止（stop）：正常終止 Aspire 程序。使用方式：/launch start 或 /launch stop。
trigger: /launch
---

# /launch

| 指令 | 行為 |
|---|---|
| `/launch start` | 跑 `scripts/launch-start.ps1`，成功後開瀏覽器截圖 |
| `/launch stop`  | 跑 `scripts/launch-stop.ps1` |
| `/launch`（無 args） | 報錯：必須指定 start 或 stop，結束 |

所有重邏輯（PID 管理、ready 偵測、四象限判斷、失敗清理）封裝在兩支 PowerShell script 內。本檔只負責呼叫 script、依結果碼處理。

PID 暫存於 `.claude/tmp/{aspire,tailwind}.pid`；log 寫入 `.claude/tmp/{aspire-startup,tailwind-watch}.log`。

---

## /launch start

1. 以 Bash tool 執行：
   ```
   pwsh -NoProfile -File .claude/skills/launch/scripts/launch-start.ps1 -Url https://blazorn10wasmlab.dev.localhost:7009/
   ```
2. 看 stdout 第一個 token：

| 結果 | 處理 |
|---|---|
| `LAUNCH_OK url=... aspire_pid=... tailwind_pid=...` | 解析 url，依序呼叫 chrome MCP 三步：`tabs_context_mcp` → `tabs_create_mcp`（或既有 tab 用 `navigate`）帶 url → `take_screenshot`。回報「✅ 應用程式已就緒 url=...」並附截圖。若任一 chrome 步驟失敗，回報「✅ 服務已啟動 url=...，但瀏覽器開啟失敗：{原因}」（不影響 start 成功）。 |
| `LAUNCH_FAIL reason=already_running aspire_pid=X tailwind_pid=Y` | 回報「❌ session 已在執行中（aspire PID X, tailwind PID Y）。請先 /launch stop」。**不自動 stop。** |
| `LAUNCH_FAIL reason=tailwind_cli_missing` | 回報「❌ 找不到 .tools/tailwindcss.exe，請依 CLAUDE.md 下載」。 |
| `LAUNCH_FAIL reason=tailwind_timeout` | 回報「❌ Tailwind watch 啟動逾時，請查 .claude/tmp/tailwind-watch.log」。 |
| `LAUNCH_FAIL reason=aspire_timeout` | 回報「❌ Aspire 啟動逾時，請查 .claude/tmp/aspire-startup.log」。 |

任何 `LAUNCH_FAIL` 都**不重試、不自動修復**，直接結束。

---

## /launch stop

1. 以 Bash tool 執行：
   ```
   pwsh -NoProfile -File .claude/skills/launch/scripts/launch-stop.ps1
   ```
2. 看 stdout 第一個 token：

| 結果 | 處理 |
|---|---|
| `STOP_OK aspire=... tailwind=...` | 回報「✅ session 已結束（aspire={state}, tailwind={state}）」。state 可為 `killed` / `stale_cleaned` / `absent`。 |
| `STOP_FAIL reason=no_session` | 回報「找不到執行中的 session」。 |

---

## /launch（無 args）

不執行任何 script，直接回報「請指定 start 或 stop」後結束。
