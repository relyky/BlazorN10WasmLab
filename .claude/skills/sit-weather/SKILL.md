---
name: sit-weather
description: SIT 基本測試第一項：開啟 Weather 頁面，依序點擊「刷新（Unary）」與「刷新（Streaming）」按鈕，確認 gRPC-Web 通訊正常、資料每次刷新都有更新。
trigger: /sit-weather
---

# /sit-weather

**SIT Test #1 — Weather gRPC-Web 通訊驗證**

測試目標：確認 Weather 頁面的兩種 gRPC-Web 呼叫模式皆能正常取得資料並更新畫面。

## 前置條件

應用程式必須已啟動。若尚未啟動，請先執行 `/launch start`。

## 執行步驟

### Step 1 — 開始 GIF 錄製

使用 `mcp__claude-in-chrome__gif_creator`（`action: start_recording`）開始錄製操作過程。

### Step 2 — 導覽至 Weather 頁面

使用 `mcp__claude-in-chrome__computer` 截一張初始畫面，然後點擊左側導覽列的 **Weather** 項目（座標約 `[91, 207]`）。

等待 2 秒，截圖確認 Weather 頁面已載入（應顯示資料表格及兩個刷新按鈕）。

### Step 3 — 測試「刷新（Unary）」

1. 記錄目前表格的第一筆資料（Date / Temp.C / Summary）
2. 點擊「刷新（Unary）」按鈕（座標約 `[341, 187]`）
3. 等待 2 秒，截圖
4. 確認表格資料已更新（與刷新前不同）→ ✅ Unary 正常

### Step 4 — 測試「刷新（Streaming）」

1. 記錄目前表格的第一筆資料
2. 點擊「刷新（Streaming）」按鈕（座標約 `[499, 187]`）
3. 截圖（捕捉串流中間狀態，可能只顯示部分資料）
4. 等待 2 秒，再次截圖
5. 確認表格最終顯示完整 5 筆資料且內容已更新 → ✅ Streaming 正常

### Step 5 — 停止錄製並匯出 GIF

使用 `mcp__claude-in-chrome__gif_creator` 依序：
1. `action: stop_recording`
2. `action: export`，`filename: sit_weather_test.gif`，`download: true`

### Step 6 — 回報測試結果

以表格格式回報：

| 測試項目 | 說明 | 結果 |
|---|---|---|
| Weather 頁面載入 | 頁面顯示資料表格與刷新按鈕 | ✅ / ❌ |
| 刷新（Unary） | 一次取回全部資料，畫面更新 | ✅ / ❌ |
| 刷新（Streaming） | 串流方式逐筆接收，畫面更新 | ✅ / ❌ |

若任何步驟失敗，回報錯誤訊息或截圖異常內容。
