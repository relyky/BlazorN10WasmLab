## Context

前一個 change `enable-launch-hot-reload` 將 `/launch start` 切換為 `dotnet watch`，但實機驗證時觀察到：

- 編輯 `Counter.razor` 加入 `<span class="text-sky-400 text-[3em] font-bold">` 後，`dotnet watch` log 印出 `🔥 C# and Razor changes applied in 2363ms`，但畫面只有 `font-bold` 生效（已存在於 css），新增的 `text-sky-400` 與 `text-[3em]` 完全沒效果。
- `grep "text-sky-400" BlazorN10WasmLab/BlazorN10WasmLab/wwwroot/app.css` 回傳 0 — 確認 utility 未被生成。

根本原因：Tailwind CLI 透過 MSBuild target 在 build 時執行（見 CLAUDE.md 描述）。`dotnet watch` 的 Hot Reload 路徑不觸發完整 MSBuild，只 patch 載入中的 assembly。因此 Tailwind 沒有重跑機會。

`.tools/tailwindcss.exe` standalone CLI 支援 `--watch` 旗標，可常駐監聽 input + scan 來源檔，輸出變更時即時寫入 output。這是官方推薦的開發模式。

## Goals / Non-Goals

**Goals:**
- `/launch start` 一次啟動後，編輯 `.razor` 加入新 Tailwind utility 即可在瀏覽器看到效果。
- 兩個並行的 watch 程序（Aspire + Tailwind）互不阻塞。
- `/launch stop` 乾淨終止兩者。

**Non-Goals:**
- 不重構 Tailwind 的 MSBuild 整合（build / publish 仍走 MSBuild target，與 watch 是兩條獨立路徑）。
- 不引入 npm 或 Node.js 工具鏈。
- 不處理 hot CSS replace 至瀏覽器的細節（依賴 Blazor / 瀏覽器自身偵測 `app.css` 變更）。
- 不改 Aspire AppHost 程式碼。

## Decisions

### Decision 1：以獨立背景 task 跑 Tailwind CLI，與 Aspire watch 並行

而非整合進 `dotnet watch` 的 lifecycle hook（過於複雜，且 MSBuild watch hook 設定深）。

最終指令（在 SKILL Step 2 啟動 Aspire 前先啟動）：
```bash
./.tools/tailwindcss.exe -i BlazorN10WasmLab/Styles/app.css -o BlazorN10WasmLab/BlazorN10WasmLab/wwwroot/app.css --watch 2>&1 | tee .claude/tmp/tailwind-watch.log
```

寫入 task_id 至 `.claude/tailwind-watch_session.tmp`。

**為何先啟動 Tailwind**：Aspire 啟動需要 `wwwroot/app.css` 已存在（首次 build 由 MSBuild target 生成）；之後 Tailwind watch 接手後續更新。實務上若 `wwwroot/app.css` 已是最新狀態，Tailwind 啟動瞬間 idle，不衝突。

### Decision 2：仍保留 MSBuild target 的初次生成

不修改 `.csproj` 的 Tailwind target — 它在 `dotnet build` / `dotnet publish` 仍負責生成（CI、Release 場景必要）。watch 模式只作用於 dev session。

### Decision 3：兩個 task 各用獨立暫存檔

- Aspire: `.claude/aspire-debug_session.tmp`（已存在）
- Tailwind: `.claude/tailwind-watch_session.tmp`（新增）

`/launch stop` 讀兩份檔、各自 `TaskStop`，全清空。任一失敗 log error 但繼續處理另一個。

**替代方案**：合併一個 json 檔（如 `.claude/launch-session.tmp` 含 `{"aspire": "...", "tailwind": "..."}`）— 但 read/write 變複雜、與既有 `.tmp` 純文字格式不一致，捨棄。

## Risks / Trade-offs

- **[Risk] Tailwind watch 啟動失敗（如 CLI 不存在）會卡住 `/launch start`** → Mitigation: 啟動前先 `test -f .tools/tailwindcss.exe`，不存在則明確報錯並停止流程；不啟 Aspire。
- **[Risk] 同時兩個 stdout 寫不同 log，user 排查時要看兩個 log** → Mitigation: SKILL 在啟動成功訊息中明示兩個 log 路徑；錯誤時 tail 兩者。
- **[Risk] Tailwind watch 偶有「未偵測到 .razor 變更」的情況**（CLI 對 .razor scan 通常 ok，但有版本 bug 史） → Mitigation: 觀察後若發生，加 `--content "**/*.razor"` 顯式指定 source，但目前 v4 預設配置已涵蓋。
- **[Risk] 兩個 watch 都修改 `wwwroot/app.css`（MSBuild 也會寫）會發生爭用** → Mitigation: dev session 期間使用者不應再跑 `dotnet build`；若真跑了，最後 watch 仍會修正。風險可接受。

## Migration Plan

1. 更新 SKILL.md：新增 Tailwind 啟動 / 暫存檔寫入步驟（Step 2 之前或之中）。
2. 更新 SKILL.md：`/launch stop` 同時讀兩份 tmp、stop 兩個 task。
3. 本地驗證：`/launch stop` 後 `/launch start`，編輯 `.razor` 加入新 utility，瀏覽器數秒內看到效果。
4. 無 rollback 機制需求，git revert SKILL.md 即還原。
