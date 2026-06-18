# PRD：移轉 RAG 後端服務至 Server 專案

- **狀態（triage）**：`ready-for-agent`

## Problem Statement

開發者在另一個 InteractiveServer Blazor 專案裡有一套已可運作的自管 RAG 後端：文件擷取（ingestion）、向量儲存、語意搜尋、與 LLM 串流對話。開發者希望把這套後端能力帶進本專案，作為後續在 WebAssembly 前端建置 AI 聊天功能的基礎。

困難在於兩專案的 render mode 不同：來源服務由 Razor 元件在 Server 端直接 DI 注入，而本專案全域採 WebAssembly——UI 在瀏覽器執行。瀏覽器沙盒不可能持有 OpenAI API key、開啟 sqlite 連線、或執行 PDF 抽字，因此這些後端服務只能存在於 Server 端。開發者需要先把後端碼乾淨落地到 Server 專案，而非一次到位接通前端。

## Solution

把來源的後端服務移轉到本專案 Server 端的服務目錄，碼邏輯維持不變，僅調整命名空間以符合本專案慣例。連同必要的套件相依、相依注入接線、與範例文件一併帶入，使其在 Server 端能編譯、能啟動。

本階段不接前端：不設計 gRPC 合約、不更動 Contracts 與 Client。WebAssembly 前端如何透過 gRPC-Web 呼叫這些服務（特別是 LLM 串流如何映射為 gRPC streaming）是後續的獨立工作。既有的 Weather 範例服務完全保留，作為下一階段串流合約的範本。

## User Stories

1. 作為開發者，我想把 LLM 串流對話服務（OpenAI Responses API 串流、多輪 tool-call 狀態機、結構化追問建議）搬進 Server 專案，以便日後前端能透過 gRPC 取得對話能力。
2. 作為開發者，我想把語意搜尋門面搬進 Server，以便日後能對已擷取的 chunk 做帶來源文件過濾的檢索。
3. 作為開發者，我想把 chunk 資料模型搬進 Server，作為檢索結果與引用（citation）的資料載體。
4. 作為開發者，我想把文件擷取流程（讀檔、依副檔名分流抽字、固定 token 切塊、批次計算 embedding、整批重建向量庫）搬進 Server，以便 ingestion 能在 Server 執行。
5. 作為開發者，我想把自管的向量庫存取（sqlite-vec 虛擬表、KNN 查詢）搬進 Server，以便在 Server 端持久化與檢索向量。
6. 作為開發者，我想讓所有搬入的服務採用本專案的服務命名空間慣例，以便與既有服務一致、避免混雜外來命名前綴。
7. 作為開發者，我想把來源的套件相依以相同版本帶入，以便搬入的碼無需任何邏輯改動即可編譯。
8. 作為開發者，我想在 Server 啟動時照既有方式建構 OpenAI 用戶端（從設定與 user-secrets 讀取金鑰與端點），以便服務能取得對話與 embedding 用戶端。
9. 作為開發者，我想把所有 RAG 服務註冊進 Server 的相依注入容器，以便它們能被解析與注入。
10. 作為開發者，我想把來源的範例文件（一份 PDF、一份 Markdown）搬進 Server 的靜態資源目錄，以便 ingestion 有實際內容可讀，且同時覆蓋兩種抽字路徑。
11. 作為開發者，我想在移轉後確認 sqlite-vec 的原生擴充能在 Server 行程載入，以便排除「編譯通過但執行期載不動原生元件」的潛伏風險。
12. 作為開發者，我想讓既有的 Weather 範例服務與其 gRPC-Web 接線不受影響，以便既有功能持續運作，並作為下一階段串流合約的範本。
13. 作為維護者，我想把這次釐清的 RAG 領域詞彙登錄進專案的領域語言文件，以便團隊與 AI 後續使用一致的術語。

## Implementation Decisions

- **服務落點**：所有後端服務搬入 Server 專案的服務目錄，保留 ingestion 與 vector-store 兩個子分組的結構。
- **唯一允許的修改**：命名空間與檔間引用由來源前綴改為本專案的服務命名空間前綴。服務的方法簽章、邏輯、註解一律不動。
- **套件版本**：照搬相同版本，包含 OpenAI SDK、Microsoft.Data.Sqlite、sqlite-vec、PDF 抽字、ML tokenizer 系列、Bcl.Memory。不升版（升版等同改碼）。先全帶以求編譯通過，多餘者於編譯後再精簡。
- **OpenAI 設定**：照搬既有方式，從設定與 user-secrets 讀取金鑰與端點，缺值在啟動時拋例外（屬預期行為）。本階段不引入 Aspire 資源編排——那是獨立決策。
- **相依注入**：註冊 embedding 用戶端、向量庫、文件擷取器、語意搜尋、對話用戶端、串流對話服務，以及指向範例文件目錄的 keyed 服務。
- **範例文件**：放入 Server 靜態資源目錄，由既有靜態資源管線處理，毋須額外的內容複製設定。
- **既有資產**：Weather 範例服務、其合約與 Client 端一律不動，新服務與其並存。
- **範圍邊界**：不設計 gRPC 合約、不更動 Contracts 與 Client、不搬任何資料庫檔。
- **領域詞彙登錄**：領域語言文件新增 RAG ingestion、chunk、vector store、semantic search、串流對話／tool-call、follow-up suggestions 等條目（純概念定義，無實作細節）。

## Testing Decisions

好的測試只驗外部行為，不綁實作細節。本階段為「只搬不改」的搬遷，沒有新行為，因此驗收以編譯與啟動為準，不新增單元測試（測試的移轉留待後續階段）：

- **編譯驗證**：建置整個方案應通過，確認搬入的服務、套件相依、相依注入接線皆無誤。
- **sqlite-vec 原生載入驗證**：啟動 Server，確認向量庫開啟連線時載入原生擴充不拋例外。這是唯一「編譯通過不等於可執行」的風險點——其餘服務皆為純受控碼，編譯通過幾近等於可執行；唯向量庫依賴原生擴充，須實際啟動才能確認。
- **回歸**：既有的 Weather 與 Counter 互動測試（既有 SIT 流程）仍正常，確認移轉未破壞既有功能。
- **prior art**：本專案既有的 SIT 流程（Weather、Counter）即是「啟動應用、觀察行為」式驗證的先例，本次沿用同樣的啟動驗證取向。

## Out of Scope

- gRPC 合約設計、Contracts 與 Client 端、任何 WebAssembly 前端。
- LLM 串流如何映射為 gRPC-Web streaming。
- 服務碼邏輯改動、套件升版、Aspire 資源編排。
- 串流對話服務與語意搜尋的單元測試（依賴外部 OpenAI API）。
- 既有 Weather 範例服務的任何變更。

## Further Notes

- 唯一「編譯通過不等於可執行」的風險點是向量庫的 sqlite-vec 原生擴充載入，故特別納入啟動驗收。
- 部分 tokenizer 資料套件與記憶體 polyfill 套件在本專案的目標框架下可能多餘，列為編譯後的精簡候選，先不預先排除。
- 下一階段建議直接照既有 Weather 範例服務的 gRPC-Web 串流範式，把 LLM 串流對話接成 Contracts 服務。
