## 1. Contracts — gRPC 合約

- [x] 1.1 新增聊天服務介面（`[ServiceContract]`）：server-streaming 串流方法 + unary 追問建議方法
- [x] 1.2 新增扁平資料模型（`[ProtoContract]`）：串流請求、串流回覆（含類別列舉：逐字文字／工具提示／回合狀態）、建議請求、建議回覆
- [x] 1.3 確認資料模型仿既有 Weather 扁平模型，無多型階層

## 2. Server — 聊天服務實作

- [x] 2.1 新增聊天 gRPC 服務（namespace `BlazorN10WasmLab.Services`，實作合約介面）
- [x] 2.2 服務內組裝「載入文件」「語意搜尋」兩個工具（注入既有語意搜尋），搬入系統提示詞與建議提示詞
- [x] 2.3 串流方法：呼叫既有串流對話、把內部事件映射成扁平串流回覆（含搜尋參數解析、回合狀態事件）
- [x] 2.4 建議方法：呼叫既有建議能力，回建議集合
- [x] 2.5 `Program.cs` 註冊並 EnableGrpcWeb（仿既有 Weather 服務那行），確認既有接線未動

## 3. TDD — 純邏輯單元測試（紅綠燈）

- [x] 3.1 RED→GREEN：串流事件扁平化映射（內部事件 → 扁平回覆，含 Search 參數解析）的單元測試與實作
- [x] 3.2 RED→GREEN：前端對話脈絡縮減（取最近數則非空訊息）的單元測試與實作

## 4. Client — gRPC client

- [x] 4.1 新增具體 client 類別實作合約介面，用低階呼叫器（仿既有 Weather client）：串流用 server-streaming call、建議用 unary call
- [x] 4.2 `Client/Program.cs` 註冊 client 至 DI

## 5. Client — UI 元件移轉

- [x] 5.1 搬入訊息模型（含工具提示標記、角色列舉）
- [x] 5.2 搬入七個 UI 元件（標頭、訊息串、單則訊息、建議列、輸入框、引用卡片、載入動畫）連同各自 scoped 樣式
- [x] 5.3 聊天頁改注入 gRPC client（路由 `/chat`）：串流消費取代直接呼叫後端；工具組裝／系統提示詞移除（已在 Server）；回合狀態從串流事件取得
- [x] 5.4 建議元件改注入 gRPC client，呼叫建議方法；脈絡縮減留前端（ChatContextReducer）
- [x] 5.5 調整 JS import 路徑為前端對應靜態路徑

## 6. Client — 靜態資源

- [x] 6.1 搬入第三方 lib（marked、dompurify、markdown_viewer、pdf_viewer、pdfjs-dist）至 Server wwwroot（host 同源），不搬來源 tailwind preflight
- [x] 6.2 搬入自訂 web component 腳本（app.js），於 Server App.razor 以 module 載入
- [x] 6.3 全域共用樣式併入 Server Styles/app.css（漸層、page-width、btn-*）；NavMenu 加 Chat 連結

## 7. 編譯與端到端驗證

- [x] 7.1 `dotnet build BlazorN10WasmLab.slnx` 通過（三專案 + 合約，0 錯誤）
- [x] 7.2 啟動，端到端 SIT：送訊息→逐字串流→工具卡片→引用卡片→點擊開檢視器→追問建議→New chat（需金鑰 + 瀏覽器，手動）
- [x] 7.3 既有 Weather／Counter SIT 仍正常（手動）
- [x] 7.4 視覺對照來源，確認儘可能一致（手動）
