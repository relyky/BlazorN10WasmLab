## Why

`app-shell/spec.md`「左側可折疊側邊欄導覽」Requirement 的群組折疊/展開 Scenario 未明文指定動畫，實際操作時 NavMenu 群組折疊瞬間生效（DOM add/remove），與一般 UX 期待落差。根因是 `@if (_groupOpen) { ... }` 條件渲染——DOM 直接增刪，無 CSS 過渡時機。

> **Scope 縮減備註**：本 change 原規劃同時修正 MainLayout 桌面收合動畫（用負 margin 取代 `md:hidden`），但驗證時在當前環境下出現 sidebar 整體失效情況，已 revert MainLayout 改動，桌面收合動畫議題另案處理。本 change 僅涵蓋 NavMenu 群組折疊動畫。

## What Changes

- 修正 `NavMenu.razor` 群組折疊動畫：移除 `@if (_groupOpen)` 包裹，子清單常駐渲染，靠 class 切換 `max-h-0` ↔ `max-h-64` + `overflow-hidden transition-all duration-200` 實現平滑展開/收合。
- 補強 `openspec/specs/app-shell/spec.md`：「左側可折疊側邊欄導覽」Requirement 新增「群組折疊平滑動畫」Scenario。

## Capabilities

### New Capabilities
（無）

### Modified Capabilities
- `app-shell`：「左側可折疊側邊欄導覽」Requirement 擴充 — 群組折疊行為明定為平滑動畫（非瞬間 toggle）。

## Impact

- **程式碼**：1 個 .razor 檔（NavMenu.razor），移除一層 `@if` + class 字串調整。
- **Spec**：1 份當前 spec 加 1 個 Scenario（delta 走 MODIFIED Requirement）。
- **無行為破壞**：群組展開/折疊語意完全不變；只是過渡視覺從瞬間變為 200ms 平滑。
- **無套件相依、無 wire 變更、無 API 變更**。
