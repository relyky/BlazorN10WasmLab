## 1. 套件相依

- [x] 1.1 在 Server 專案 `.csproj` 加入八個 NuGet 套件（相同版本）：OpenAI、Microsoft.Data.Sqlite、sqlite-vec、PdfPig、Microsoft.ML.Tokenizers 及兩個 tokenizer 資料套件、Microsoft.Bcl.Memory
- [x] 1.2 `dotnet restore` 確認套件還原成功

## 2. 搬入服務碼

- [x] 2.1 把九個服務檔複製進 Server 專案服務目錄，保留 ingestion 與 vector-store 子目錄結構
- [x] 2.2 將每檔命名空間由來源前綴改為本專案服務命名前綴（含 .Ingestion / .VectorStore 子命名空間）
- [x] 2.3 修正檔間 `using`，確認服務邏輯、方法簽章、註解一字不改

## 3. 範例文件

- [x] 3.1 把來源範例文件（PDF + Markdown）複製進 Server 靜態資源目錄（不搬任何 .db 檔）

## 4. Server 啟動接線

- [x] 4.1 在 Server `Program.cs` 加入 OpenAI 用戶端建構：從設定與 user-secrets 讀金鑰與端點，缺值拋例外
- [x] 4.2 建立 embedding 用戶端與向量庫連線字串（指向執行期基底目錄的 db 檔）
- [x] 4.3 註冊 RAG 服務 DI：向量庫、文件擷取器、語意搜尋、對話用戶端、串流對話服務，及指向範例文件目錄的 keyed 服務
- [x] 4.4 確認既有 gRPC-Web 與 Weather 服務接線未被更動

## 5. 編譯與啟動驗證

- [x] 5.1 `dotnet build` 整個方案通過（服務、套件、DI 接線無誤）
- [x] 5.2 於 user-secrets 設定 `AzureOpenAI:ApiKey` 與 `AzureOpenAI:EndPoint`（需使用者提供金鑰，手動執行）
- [x] 5.3 啟動 Server，確認向量庫開啟連線載入 sqlite-vec 原生擴充不拋例外（由 SqliteVecStoreTests round-trip 實質驗證：native 載入 + rebuild + KNN 全通過）
- [x] 5.4 執行既有 SIT 流程（Weather、Counter），確認既有功能未被破壞（需啟動瀏覽器，手動執行）

## 6. 領域詞彙登錄

- [x] 6.1 在領域語言文件新增 RAG 詞彙：ingestion、chunk、vector store、semantic search、串流對話／tool-call、follow-up suggestions（純概念定義，無實作細節）

## 7. 編譯後精簡（可選）

- [x] 7.1 依編譯 warning 評估並移除多餘套件：build 為 0 警告，無明確多餘訊號；依「先求綠燈再精簡」與 MVP 原則保留現狀，不冒險移除
