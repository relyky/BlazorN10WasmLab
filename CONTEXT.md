# CONTEXT

本檔案累積專案的領域語言（domain vocabulary）。新術語在被引入時即時登錄，避免散落於 commit message 與註解。

## 詞彙

- **Viewport breakpoint** — 視窗寬度跨越 Tailwind `md:`（768px）斷點的事件來源。由 `IViewportBreakpoint` 提供，封裝 `window.matchMedia` 互動，使元件能以「目前是不是 mobile」單一概念回應 layout 變化。對應 capability `viewport-breakpoint`（`openspec/specs/viewport-breakpoint/spec.md`）。
- **Ingestion（文件擷取）** — 把來源文件轉成可檢索狀態的流程：讀檔、切塊、計算向量、寫入向量庫。對應 capability `rag-backend`。
- **Chunk（區塊）** — 文件被切成的一段可檢索文字單位，帶來源文件識別與唯一鍵，是檢索結果與引用（citation）的基本載體。
- **Vector store（向量庫）** — 持久化 chunk 向量並支援最近鄰（KNN）查詢的儲存層。本專案自管 sqlite-vec。
- **Semantic search（語意搜尋）** — 以查詢文字的向量在向量庫做最近鄰檢索、取回相關 chunk 的能力，可選依來源文件過濾。
- **串流對話（streaming reply）** — 與 LLM 的一回合對話，逐段串流回應文字（打字機效果），過程中模型可發起 tool-call 取得外部資訊後再續答。
- **Tool-call（工具呼叫）** — 對話中模型決定呼叫某個本地函式並帶參數的動作；其執行結果回餵模型以繼續生成。
- **Follow-up suggestions（追問建議）** — 依當前對話脈絡產生的數條後續提問建議，供使用者一鍵延續對話。
