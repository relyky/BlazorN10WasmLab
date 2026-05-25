## 1. MainLayout 改寫

- [x] 1.1 重寫 `MainLayout.razor` 的 `SidebarClass` getter：抽出共用 base class，只讓開啟態與關閉態差在 `translate-x-0`/`-translate-x-full` 與 `md:ml-0`/`md:-ml-60`
- [x] 1.2 把 `transition-transform` 換成 `transition-[transform,margin]`，並僅在 `_initialized == true` 時掛上
- [x] 1.3 移除關閉態的 `md:hidden`

## 2. 首次 hydrate 不帶動畫

- [x] 2.1 在 `MainLayout.razor` 元件加入 `private bool _initialized = false;`
- [x] 2.2 修改 `OnAfterRenderAsync`：完成 `_sidebarOpen` 初始化後設 `_initialized = true` 並呼叫 `StateHasChanged`
- [x] 2.3 確認 `SidebarClass` 根據 `_initialized` 決定要不要含 transition utility

## 3. 驗證

- [x] 3.1 `dotnet build` 確認 Tailwind 產出的 `wwwroot/app.css` 包含 `.-ml-60`（或 `md:-ml-60` 對應規則）與 `.transition-\[transform\,margin\]`
- [x] 3.2 `/launch start`，在桌機寬度（≥ 768px）視窗點 Logo 收合 → 觀察 sidebar 200ms 向左滑出、main 同步展開
- [x] 3.3 桌機點 Logo 展開 → 觀察 sidebar 200ms 從左滑入、main 同步收縮
- [x] 3.4 縮窗到手機寬度（< 768px）→ 驗證手機 overlay 滑入/滑出維持 200ms 動畫、遮罩正常
- [x] 3.5 重新整理頁面（桌機與手機各一次）→ 確認初始狀態直接呈現，無 200ms 跳變
- [x] 3.6 點任一 NavLink → 桌機保持展開、手機自動收合，行為與既有規格一致
