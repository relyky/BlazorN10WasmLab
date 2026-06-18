## Why

開發者在另一個 InteractiveServer Blazor 專案有一套可運作的自管 RAG 後端（文件擷取、向量儲存、語意搜尋、LLM 串流對話），希望帶進本專案作為日後 WebAssembly 前端 AI 聊天功能的基礎。本專案全域採 WebAssembly，瀏覽器沙盒不可能持有 API key、開 sqlite 連線或執行 PDF 抽字，因此這套後端必須先乾淨落地到 Server 端，後續才接前端。

## What Changes

- 將來源後端服務（LLM 串流對話、語意搜尋門面、chunk 資料模型、文件擷取流程、自管 sqlite-vec 向量庫）移轉至 Server 專案的服務目錄，**碼邏輯不變**，僅調整命名空間為本專案的服務命名慣例。
- 以**相同版本**帶入來源的套件相依（OpenAI SDK、Microsoft.Data.Sqlite、sqlite-vec、PDF 抽字、ML tokenizer 系列、Bcl.Memory），不升版。
- 在 Server 啟動接線：照既有方式從設定與 user-secrets 建構 OpenAI 用戶端，並把 RAG 服務註冊進相依注入容器（含指向範例文件目錄的 keyed 服務）。
- 將來源範例文件（一份 PDF、一份 Markdown）放入 Server 靜態資源目錄，覆蓋兩種抽字路徑。
- 於領域語言文件登錄 RAG 相關詞彙（ingestion、chunk、vector store、semantic search、串流對話／tool-call、follow-up suggestions）。
- 本階段**不接前端**：不設計 gRPC 合約、不更動 Contracts 與 Client；既有 Weather 範例服務完全保留。

## Capabilities

### New Capabilities

- `rag-backend`: Server 端的檢索增強生成後端能力——文件擷取與切塊、向量持久化與 KNN 檢索、語意搜尋門面、以及具多輪 tool-call 的 LLM 串流對話。本階段僅在 Server 端可被相依注入解析與啟動，尚未對外暴露 gRPC 端點。

### Modified Capabilities

<!-- 無。本變更不更動既有 capability 的 requirement；既有 grpc-web-communication 與 Weather 範例服務不受影響。 -->

## Impact

- **新增程式**：Server 專案服務目錄新增 RAG 服務群（含 ingestion 與 vector-store 子分組）。
- **相依**：Server 專案 `.csproj` 新增八個 NuGet 套件（相同版本）。
- **啟動接線**：Server `Program.cs` 新增 OpenAI 用戶端建構與 RAG 服務 DI 註冊。
- **設定**：需於 user-secrets 設定 `AzureOpenAI:ApiKey` 與 `AzureOpenAI:EndPoint`（缺值啟動時拋例外，屬預期）。
- **靜態資源**：Server 靜態資源目錄新增範例文件。
- **文件**：領域語言文件新增 RAG 詞彙。
- **風險點**：sqlite-vec 原生擴充載入是唯一「編譯通過不等於可執行」之處，須啟動驗證。
- **不受影響**：既有 Weather／Counter 功能、gRPC-Web 接線、Contracts、Client。
