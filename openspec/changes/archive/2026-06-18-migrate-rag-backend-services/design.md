## Context

來源是一套在 InteractiveServer Blazor 上運作的自管 RAG 後端，服務由 Razor 元件直接 DI 注入。本專案全域採 WebAssembly render mode、Client-Server 走 gRPC-Web。核心張力：後端服務需要 OpenAI 金鑰、sqlite 連線、PDF 抽字與原生擴充，這些**只能在 Server 端**，瀏覽器沙盒無法承載。

經 grilling 確定範圍為「只搬不改」：把後端碼乾淨落地到 Server 專案，編譯通過、DI 接好、啟動驗證，**不接前端**。前端如何透過 gRPC-Web 呼叫（尤其串流映射）是後續獨立工作。

## Goals / Non-Goals

**Goals:**

- 後端服務原樣落地 Server 端，碼邏輯不變，僅改命名空間。
- 套件、DI、範例文件齊備，方案可編譯。
- 啟動時驗證 sqlite-vec 原生擴充可載入。
- 既有 Weather／Counter 功能與 gRPC-Web 接線零影響。

**Non-Goals:**

- gRPC 合約設計、Contracts／Client／任何 WASM UI。
- 串流對話如何映射為 gRPC-Web streaming。
- 服務邏輯改動、套件升版、Aspire 資源編排。
- 串流對話與語意搜尋的單元測試（依賴外部 API）。

## Decisions

**D1：服務留在 Server 專案，不下放 Client。**
理由：瀏覽器無法持有金鑰、開 sqlite、跑原生擴充。替代方案（在 Client 端呼叫）在 WASM 沙盒不可行，直接排除。

**D2：命名空間改為本專案服務前綴（含 .Ingestion / .VectorStore 子分組）。**
理由：碼成為本專案的一部分就該守本專案慣例；保留外來前綴會讓人困惑歸屬。替代方案（保留來源前綴）改動最小但破壞一致性，捨棄。

**D3：套件照搬相同版本，不升版。**
理由：來源碼針對這些確切版本撰寫（特別是實驗性 Responses API 型別在版本間會變），升版等同改碼，違反「只搬不改」。先全帶求綠燈，編譯後再精簡多餘者（部分 tokenizer 資料與記憶體 polyfill 套件在目標框架下可能多餘）。

**D4：OpenAI 設定照搬原樣，不引入 Aspire 編排。**
理由：本階段目標是讓服務能跑，最小變動最安全。Aspire 資源編排是獨立決策，混入會擴大範圍、失焦。

**D5：範例文件一起搬，放靜態資源目錄。**
理由：兩個檔（PDF + Markdown）恰好覆蓋兩種抽字路徑，使 ingestion 有實際內容可驗。由既有靜態資源管線處理，毋須額外內容複製設定。

**D6：既有 Weather 範例服務完全保留。**
理由：它是 gRPC-Web 串流的 working 範本，下一階段把串流對話接成 gRPC 合約時正好照抄。它横跨 Server／Client／Contracts，動它即超範圍。

## Risks / Trade-offs

- **sqlite-vec 原生擴充載入失敗** → 唯一「編譯通過不等於可執行」的風險點。其餘服務皆純受控碼，編譯過幾近可跑；唯向量庫依賴原生擴充。Mitigation：啟動驗證納入驗收，提早暴露而非潛伏到接 UI 階段。
- **套件版本含 alpha／preview（sqlite-vec、tokenizer）** → 未來升版可能 breaking。Mitigation：本階段鎖版不動；升版列為後續獨立工作。
- **範例文件 ingestion 為空或失敗** → 文件擷取器設計為無可讀文件時建空表而不崩潰，風險低。
- **多帶的套件造成相依膨脹** → Mitigation：編譯通過後精簡多餘套件。

## Migration Plan

1. 帶入八個 NuGet 套件（相同版本）至 Server 專案。
2. 搬入服務檔，調整命名空間與檔間引用，保留子目錄結構。
3. 搬入範例文件至 Server 靜態資源目錄。
4. Server 啟動接線：建構 OpenAI 用戶端、註冊 RAG 服務 DI。
5. 編譯方案；啟動 Server 驗證原生擴充載入。
6. 登錄 RAG 領域詞彙至領域語言文件。

回退：本變更為純新增（服務、套件、DI、文件），不更動既有資產，回退即移除新增項，無資料遷移風險。

## Open Questions

- 多餘套件（tokenizer 資料、記憶體 polyfill）的精簡待編譯後依實際 warning 決定。
- 下一階段串流對話的 gRPC-Web 合約形狀（事件如何映射 protobuf）留待獨立 change。
