## 1. 前置確認

- [x] 1.1 確認 `/launch` session 已停止（避免 Hot Reload 干擾），或保持運行以即時驗證
- [x] 1.2 確認 working tree 乾淨或已 stash

## 2. ~~修正 MainLayout 桌面收合動畫~~（已撤回）

> Scope 縮減備註：原規劃以負 margin 取代 `md:hidden`，但驗證時 sidebar 整體失效，已 revert MainLayout，桌面收合動畫議題另案處理。

- [x] 2.x Revert MainLayout.razor 回原 `md:hidden` + `transition-transform` 行為

## 3. 修正 NavMenu 群組折疊動畫

- [x] 3.1 編輯 `BlazorN10WasmLab/BlazorN10WasmLab.Client/Layout/NavMenu.razor`
- [x] 3.2 移除 `@if (_groupOpen)` 包裹（子清單常駐渲染）
- [x] 3.3 在子清單 `<div>` class 加入動態切換：`@(_groupOpen ? "max-h-64" : "max-h-0") overflow-hidden transition-all duration-200`（保留原有 `mt-1 ml-2 flex flex-col gap-0.5`）
- [x] 3.4 確認 chevron 旋轉動畫（`-rotate-90`）行為不變

## 4. 更新 spec 當前版本

- [x] 4.1 ~~更新 sidebar-toggle/spec.md~~（已撤回，spec 不動）
- [x] 4.2 更新 `openspec/specs/app-shell/spec.md`「左側可折疊側邊欄導覽」Requirement 描述提及平滑動畫，新增「群組折疊平滑動畫」Scenario

## 5. 驗證

- [x] 5.1 `dotnet build BlazorN10WasmLab.slnx` 成功，無警告無錯誤
- [x] 5.2 `/launch start` 啟動應用，sidebar 正常顯示（無 regression）
- [x] 5.3 **群組折疊驗證**：點「功能展示」群組標頭，子清單收合（class 切到 `max-h-0`）+ chevron 轉為 `>`；再點展開，子清單恢復（class 切到 `max-h-64`）+ chevron 轉為 `∨`。max-h transition 200ms 在 CSS 中設定正確
- [x] 5.4 確認群組折疊後子清單完全不可見、不佔空間（`max-h-0 overflow-hidden` 正確）
- [x] 5.5 全頁面樣式無 regression（Counter 頁面顯示正常、sidebar 4 個 NavLink 完整）

## 6. 收尾

- [ ] 6.1 `/launch stop` 結束 watch session
- [ ] 6.2 提交 commit
- [ ] 6.3 執行 `/opsx:archive` 歸檔本次 change
