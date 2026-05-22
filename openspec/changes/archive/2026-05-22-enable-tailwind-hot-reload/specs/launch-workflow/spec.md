## MODIFIED Requirements

### Requirement: Hot Reload 啟用於開發啟動

`/launch start` skill SHALL 同時以 `dotnet watch` 啟動 Aspire AppHost、並以 `tailwindcss.exe --watch` 啟動 Tailwind CLI 監聽，使下游 Blazor Server 與 WebAssembly Client 在開發期間支援 .NET Hot Reload，且**新增的 Tailwind utility class** 也能即時反映至 `wwwroot/app.css`。修改 `.razor` / `.cs` / `.css` 不需重啟整個應用程式。

#### Scenario: 啟動後修改 Razor 元件即時生效
- **WHEN** 使用者執行 `/launch start` 並在瀏覽器開啟應用程式後，編輯 `BlazorN10WasmLab.Client/Pages/Counter.razor` 並存檔
- **THEN** Hot Reload SHALL 於數秒內套用變更至瀏覽器，無需重新啟動 Aspire

#### Scenario: 啟動指令使用 dotnet watch
- **WHEN** `/launch start` 進入背景啟動步驟
- **THEN** 實際執行的指令 MUST 以 `dotnet watch` 為基礎（而非 `dotnet run`），並指向 `BlazorN10WasmLab.AppHost` 專案

#### Scenario: 新增 Tailwind utility 即時生效
- **WHEN** 使用者在 `.razor` 元件加入先前未使用過的 Tailwind utility class（如 `text-sky-400`）並存檔
- **THEN** Tailwind CLI watch MUST 在數秒內重生 `BlazorN10WasmLab/wwwroot/app.css`，且瀏覽器（在 Blazor 偵測到 CSS 變更後）MUST 反映新樣式，無需手動 `dotnet build` 或重啟應用

## ADDED Requirements

### Requirement: 並行管理 Tailwind watch 子程序

`/launch start` SHALL 啟動並追蹤 Tailwind CLI watch 子程序之 task_id（例如寫入 `.claude/tailwind-watch_session.tmp`），與 Aspire watch 並行執行，互不阻塞。

#### Scenario: Tailwind watch 程序啟動
- **WHEN** `/launch start` 流程執行
- **THEN** 一個獨立的背景 task MUST 以 `.tools/tailwindcss.exe -i BlazorN10WasmLab/Styles/app.css -o BlazorN10WasmLab/BlazorN10WasmLab/wwwroot/app.css --watch` 形式啟動，task_id 寫入暫存檔

### Requirement: stop 流程同時終止兩個 watch 程序

`/launch stop` SHALL 讀取兩份暫存檔（Aspire + Tailwind），對兩個 task_id 各執行 `TaskStop`，並清除兩份暫存檔。任一程序停止失敗時 MUST 回報明確錯誤，但仍嘗試停止另一個。

#### Scenario: 兩個 watch 程序皆終止
- **WHEN** 使用者執行 `/launch stop`
- **THEN** Aspire watch task 與 Tailwind watch task 皆 MUST 被 `TaskStop`；兩份暫存檔 MUST 被刪除

#### Scenario: 啟動失敗時清理另一個 watch
- **WHEN** `/launch start` 中其中一個 watch 程序啟動失敗（例如 dotnet watch 在 60 秒內未印出 `Distributed application started`）
- **THEN** 流程 MUST 停止已成功啟動的另一個 watch 程序，避免遺留孤兒 task
