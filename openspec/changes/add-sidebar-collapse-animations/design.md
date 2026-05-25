## Context

NavMenu 群組折疊使用 Razor `@if (_groupOpen) { ... }` 條件渲染——DOM 節點是 add/remove 而非 hide/show，CSS 沒有「該元素正在被移除」的中介狀態可掛 transition，導致折疊瞬間生效。

> **Scope 縮減備註**：原始 design 同時涵蓋 MainLayout 桌面收合動畫修復（用負 margin 取代 `md:hidden`），驗證時在當前 dev 環境下出現 sidebar 整體失效，已 revert，桌面收合動畫議題另案處理。本 design 僅保留 NavMenu 群組折疊動畫決策。

## Goals / Non-Goals

**Goals:**
- 點擊 NavMenu「功能展示」群組標頭折疊/展開：子清單 200ms 平滑收合/展開。
- spec 同步更新，未來迴歸測試可捕捉動畫被改回瞬間的問題。

**Non-Goals:**
- 不改 MainLayout 桌面/手機收合行為（保留原 `md:hidden` 與 `-translate-x-full`）。
- 不改群組開/關的狀態管理邏輯（`_groupOpen` 行為不動）。
- 不改 chevron 旋轉動畫（既有 `transition-transform` + `-rotate-90` 正常）。
- 不引入 JS interop 或 third-party 動畫庫。

## Decisions

### 決策 1：NavMenu 群組折疊用「max-height」而非「grid-rows 1fr」
- **選擇**：移除 `@if (_groupOpen)`；子清單常駐 render，class 切換 `max-h-0` ↔ `max-h-64` 配 `overflow-hidden transition-all duration-200`
- **理由**：Tailwind utility 原生支援、概念直觀、瀏覽器相容性無疑慮。當前 4 個 NavLink 約 144px，`max-h-64`（256px）為安全上限。
- **替代方案 B（grid-template-rows 0fr↔1fr）**：完美匹配內容高度免估算。駁回理由：arbitrary value、Tailwind v4 雖支援但較少見，可讀性與 MVP 原則考量下選 A。
- **替代方案 C（JS 量測高度）**：駁回理由：引入 JS interop，過度工程。

### 決策 2：transition 屬性用 `transition-all`
- **選擇**：NavMenu 子清單使用 `transition-all duration-200`
- **理由**：動畫對 max-height 變化，不是 transform，需要覆蓋 max-height 屬性。`transition-all` 一律處理，效能成本可忽略（單一元件、極短時長、唯一可動畫屬性就是 max-height）。

### 決策 3：spec delta 走 MODIFIED Requirement
- **選擇**：app-shell 既有「左側可折疊側邊欄導覽」Requirement 內補一個 Scenario。
- **理由**：群組折疊是既有 Requirement 的範疇延伸，不是獨立新功能。MODIFIED 寫法 OpenSpec archive 時可正確覆寫主 spec。

## Risks / Trade-offs

- **[NavMenu 加項目超過 256px 導致截斷]** → 未來若群組內 NavLink 超過 7 個（>256px），會被 `max-h-64` 裁掉 → 緩解：spec scenario 提及上限假設；若需擴充改 `max-h-96` 或改用 grid-rows 方案。
- **[`transition-all` 可能波及未預期屬性]** → 若元素後續加入其他可動畫屬性（color、border 等），會一起跑 200ms 過渡 → 緩解：當前 class 不含其他易變屬性；若日後出現問題再收斂為 `transition-[max-height]`。
- **[動畫迴歸難以自動測]** → 動畫視覺品質無法靠 build 驗證 → 緩解：每次改完手動 `/launch start` 點群組標頭目視確認；spec scenario 化讓未來人或 AI 看 spec 就知道應該有動畫。
