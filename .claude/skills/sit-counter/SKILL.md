---
name: sit-counter
description: SIT 基本測試第二項：開啟 Counter 頁面，點擊「Click me」按鈕數次，確認 Blazor WASM 互動元件計數器正常遞增。
trigger: /sit-counter
---

# /sit-counter

**SIT Test #2 — Counter WASM 互動驗證**

測試目標：確認 Counter 頁面的按鈕互動能正常觸發 Blazor WASM 元件狀態更新，計數器每次點擊遞增 1。

## 前置條件

應用程式必須已啟動。若尚未啟動，請先執行 `/launch start`。

## 執行步驟

### Step 1 — 導覽至 Counter 頁面

使用 `mcp__claude-in-chrome__computer` 截初始畫面，確認目前所在頁面。

若尚未在 Counter 頁面，點擊左側導覽列的 **Counter** 項目（座標約 `[89, 151]`）。

等待 1 秒，截圖確認 Counter 頁面已載入（應顯示「Current count: 0」與「Click me」按鈕）。

### Step 2 — 確認初始狀態

截圖記錄初始計數值（預期為 **Current count: 0**）。

> 若計數不為 0，表示頁面上次未重新載入。直接進行下一步，記錄當前值即可。

### Step 3 — 點擊「Click me」三次

依序點擊「Click me」按鈕（座標約 `[325, 188]`）三次，每次點擊後觀察計數值是否遞增。

可使用 `browser_batch` 一次批次送出三次點擊再截圖，提升效率：

```
左鍵點擊 (325, 188) → 左鍵點擊 (325, 188) → 左鍵點擊 (325, 188) → 截圖
```

### Step 4 — 驗證結果

截圖確認計數值，預期為 **Current count: 3**（或初始值 + 3）。

- 若計數正確遞增 → ✅ WASM 互動正常
- 若計數未變化或異常 → ❌ 記錄錯誤截圖與訊息

### Step 5 — 回報測試結果

以表格格式回報：

| 測試項目 | 說明 | 結果 |
|---|---|---|
| Counter 頁面載入 | 頁面顯示計數器與 Click me 按鈕 | ✅ / ❌ |
| 按鈕點擊互動 | 每次點擊計數遞增 1 | ✅ / ❌ |
| 最終計數值 | 初始值 + 3 | ✅ / ❌ |

若任何步驟失敗，回報錯誤訊息或截圖異常內容。
