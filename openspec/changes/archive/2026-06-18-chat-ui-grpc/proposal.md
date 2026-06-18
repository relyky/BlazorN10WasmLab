## Why

RAG 後端服務已搬進 Server 端但尚未對外暴露，WebAssembly 前端無法使用，也還沒有任何聊天畫面。需要把來源專案那套成熟的聊天體驗（逐字串流、工具呼叫提示、引用卡片、追問建議）透過 gRPC-Web 接通到 WASM 前端，讓使用者能在瀏覽器與文件對話。

## What Changes

- Server 端新增聊天 gRPC 服務：內部組裝「載入文件」「語意搜尋」兩個工具、執行既有串流對話，把事件以 server-streaming 串給前端；既有串流對話與語意搜尋邏輯不改。
- gRPC 合約新增於 Contracts 專案：一個 server-streaming 串流方法 + 一個 unary 追問建議方法，搭配扁平訊息 + 類別列舉。
- 前端新增具體 client 類別（仿既有 Weather client），相容 WASM 沙盒。
- 前端移轉來源整套聊天畫面元件（標頭、訊息串、單則訊息、建議列、輸入框、引用卡片、載入動畫）連同 scoped 樣式原樣搬入。
- 前端靜態資源搬入第三方函式庫（Markdown 渲染、消毒、Markdown／PDF 檢視器），畫面全功能。
- 既有 Weather 範例服務、其合約與 client 保留不動。

## Capabilities

### New Capabilities

- `chat-ui`: WebAssembly 前端的文件問答聊天能力——逐字串流回應、工具呼叫提示、引用卡片與檢視器、追問建議、New chat，透過 gRPC-Web 串接 Server 端 RAG 後端（`rag-backend`）。

### Modified Capabilities

<!-- 無。不更動既有 grpc-web-communication 與 rag-backend 的 requirement；兩者作為被依賴的既有能力。 -->

## Impact

- **Contracts**：新增聊天服務介面與扁平資料模型（含類別列舉）。
- **Server**：新增聊天 gRPC 服務實作 + `Program.cs` 註冊一行；系統提示詞與工具編排移入此服務。
- **Client**：新增具體 client + DI 註冊；新增七個 UI 元件 + 訊息模型；`wwwroot` 新增第三方 lib 與 web component 腳本。
- **依賴**：前端依賴既有 gRPC-Web 通訊基礎設施（`grpc-web-communication`）與 Server RAG 後端（`rag-backend`）。
- **不受影響**：既有 Weather／Counter 功能與 gRPC-Web 接線、已搬入的後端服務邏輯。
- **待實作中釘的小決策**：聊天頁路由落點、全域共用樣式落點、Tailwind preflight 對 Markdown 預設樣式的影響。
