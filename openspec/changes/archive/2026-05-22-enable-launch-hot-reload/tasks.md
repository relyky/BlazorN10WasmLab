## 1. 修改 launch SKILL

- [x] 1.1 在 `.claude/skills/launch/SKILL.md` Step 2 將啟動指令由 `dotnet run --project BlazorN10WasmLab.AppHost` 改為 `dotnet watch --project BlazorN10WasmLab.AppHost --non-interactive run`
- [x] 1.2 在 Step 2 加註說明：使用 `dotnet watch` 啟用 Hot Reload；`--non-interactive` 避免 rude edit 時 prompt 阻塞背景任務

## 2. 本地驗證

- [x] 2.1 若有殘留 session，先執行 `/launch stop` 清除（殘留 tmp 已清；舊 task 已不存在於 runtime）
- [x] 2.2 執行 `/launch start`，確認 60 秒內偵測到 `Distributed application started`（實測 15 秒）
- [x] 2.3 確認 Chrome 開啟 `https://blazorn10wasmlab.dev.localhost:7009/` 並顯示 首頁 / Counter / Weather 導覽列
- [x] 2.4 編輯 `BlazorN10WasmLab/BlazorN10WasmLab.Client/Pages/Counter.razor` 並存檔（多次編輯：計數值樣式、按鈕樣式、primary 色）
- [x] 2.5 切回瀏覽器，無手動重整下確認 Counter 頁面數秒內反映變更（Hot Reload 生效，log: `🔥 C# and Razor changes applied`，元件狀態保留）
- [ ] ~~2.6 編輯一個 rude edit（如在 `Counter.razor` 新增類別欄位），確認 watch 自動 restart 且不卡在 prompt~~ — 跳過（user LGTM 確認）
- [x] 2.7 執行 `/launch stop` 確認停止流程仍正常終止 task 並清除 `.claude/aspire-debug_session.tmp`

> **意外發現**：Tailwind utility class 新增時 Hot Reload 不會觸發 Tailwind CLI 重生 `wwwroot/app.css`，需手動 `dotnet build` 或使用 inline `style`。建議後續另立 change 改善（如新增 Tailwind CLI watch 並行）。

## 3. 收尾

- [x] 3.1 執行 `openspec validate --change enable-launch-hot-reload` 通過後 archive change
