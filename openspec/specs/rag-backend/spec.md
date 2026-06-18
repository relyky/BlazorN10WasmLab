# rag-backend Specification

## Purpose
TBD - created by archiving change migrate-rag-backend-services. Update Purpose after archive.
## Requirements
### Requirement: RAG 服務於 Server 端可解析

Server 專案 SHALL 在相依注入容器註冊所有 RAG 後端服務（embedding 用戶端、向量庫、文件擷取器、語意搜尋、串流對話服務，以及指向範例文件目錄的 keyed 服務），使其能被解析與注入。

#### Scenario: 容器解析 RAG 服務

- **WHEN** Server 應用程式建置完成且設定齊備
- **THEN** 語意搜尋、文件擷取、串流對話等服務皆能從相依注入容器解析，無缺漏相依

#### Scenario: 缺少 OpenAI 設定時啟動失敗

- **WHEN** 未提供 API 金鑰或端點設定
- **THEN** 應用程式於啟動時拋出明確的設定缺失例外

### Requirement: 服務採本專案命名空間慣例

搬入的 RAG 服務 SHALL 使用本專案的服務命名空間前綴（含 ingestion 與 vector-store 子命名空間），與既有服務一致，不保留來源命名前綴。

#### Scenario: 命名空間一致

- **WHEN** 檢視搬入的服務型別
- **THEN** 其命名空間皆為本專案服務命名前綴，且方法簽章與邏輯與來源相同

### Requirement: 向量庫原生擴充可載入

向量庫存取 SHALL 在 Server 行程開啟連線時成功載入 sqlite-vec 原生擴充，不拋原生載入例外。

#### Scenario: 啟動載入原生擴充

- **WHEN** Server 行程首次開啟向量庫連線
- **THEN** sqlite-vec 原生擴充載入成功，後續可建立 vec0 虛擬表

### Requirement: 文件擷取與向量檢索 round-trip

RAG 後端 SHALL 能讀取靜態資源目錄中的範例文件（Markdown 與 PDF 各一），切塊、計算 embedding、寫入向量庫，並以查詢向量做 KNN 檢索回傳最相近的 chunk。

#### Scenario: 擷取範例文件

- **WHEN** 對範例文件目錄觸發 ingestion
- **THEN** Markdown 與 PDF 皆被讀取並切成 chunk 寫入向量庫；無可讀文件時建立空表而不崩潰

#### Scenario: 語意檢索回傳 chunk

- **WHEN** 以一段查詢文字對已擷取的向量庫做檢索
- **THEN** 回傳依距離排序的最相近 chunk；提供來源文件過濾時僅回傳該文件的 chunk

### Requirement: 既有功能不受影響

本變更 SHALL NOT 更動既有 Weather 範例服務、其 gRPC-Web 接線、Contracts 或 Client。

#### Scenario: 既有 SIT 仍通過

- **WHEN** 移轉完成後執行既有的 Weather 與 Counter 互動流程
- **THEN** 兩者行為與移轉前一致

