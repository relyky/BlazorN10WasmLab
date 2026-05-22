## 1. 修改 launch SKILL — 啟動流程

- [x] 1.1 在 `.claude/skills/launch/SKILL.md` Step 2 之前，新增 Step 2a：檢查 `.tools/tailwindcss.exe` 存在，否則明確報錯並停止流程
- [x] 1.2 新增 Step 2b：以 background task 啟動 `./.tools/tailwindcss.exe -i BlazorN10WasmLab/Styles/app.css -o BlazorN10WasmLab/BlazorN10WasmLab/wwwroot/app.css --watch 2>&1 | tee .claude/tmp/tailwind-watch.log`
- [x] 1.3 取得 Tailwind task_id 並寫入 `.claude/tailwind-watch_session.tmp`
- [x] 1.4 等待 Tailwind 啟動完成訊號（log 出現 `Done in` 或 `Watching`，最多 30 秒），失敗則 stop Tailwind task 並停止流程
- [x] 1.5 在啟動成功訊息中加入 Tailwind log 路徑與 `wwwroot/app.css` 監聽狀態

## 2. 修改 launch SKILL — 停止流程

- [x] 2.1 `/launch stop` Step 1 同時讀取 `.claude/aspire-debug_session.tmp` 與 `.claude/tailwind-watch_session.tmp`
- [x] 2.2 Step 2 對兩個 task_id 各執行 `TaskStop`；任一失敗 log error，但仍嘗試另一個
- [x] 2.3 Step 3 刪除兩份暫存檔
- [x] 2.4 Step 4 回報訊息分別顯示兩個 task 狀態

## 3. 啟動失敗清理

- [x] 3.1 `/launch start` 中若 Aspire 啟動逾時，MUST 先停止已啟動的 Tailwind task，避免遺留孤兒
- [x] 3.2 反向亦然：Tailwind 啟動失敗時不啟動 Aspire（已由 Task 1.4 涵蓋）

## 4. 本地驗證

- [x] 4.1 確認無殘留 session（兩份 tmp 都不存在）
- [x] 4.2 執行 `/launch start`，確認兩個 task 均啟動，Aspire log 與 Tailwind log 都健康（Aspire task be403yi3w, Tailwind task biuu1tdyl）
- [x] 4.3 編輯 `Counter.razor` 加入先前未用過的 utility（`text-emerald-500 text-4xl`），存檔
- [x] 4.4 在數秒內確認瀏覽器套用新樣式（h1 "Counter" 顯示為大型粗體翠綠色，無需重整、無需 `dotnet build`）
- [x] 4.5 確認 `grep "text-emerald-500" BlazorN10WasmLab/BlazorN10WasmLab/wwwroot/app.css` 有結果（1 次命中；`text-4xl` 5 次命中）
- [x] 4.6 執行 `/launch stop`，確認兩個 task 都停止、兩份 tmp 都被清除

> **實作中發現**：
> 1. CLAUDE.md 的 input/output 路徑（`BlazorN10WasmLab/Styles/app.css`）與實際檔案位置（`BlazorN10WasmLab/BlazorN10WasmLab/Styles/app.css`）差一層 — proposal/design/specs/SKILL.md 原本都跟著錯，已就地修正。
> 2. Tailwind v4 CLI 在背景任務（無 stdin tty）下 `--watch` 會在首次 build 後即退出。必須改用 `--watch=always`（CLI 官方為此情境設計的旗標）。SKILL.md 已修正。

## 5. 收尾

- [x] 5.1 `openspec validate enable-tailwind-hot-reload` 通過
- [x] 5.2 archive change
