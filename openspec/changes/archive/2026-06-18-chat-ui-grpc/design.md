## Context

來源聊天頁是 InteractiveServer Blazor，元件直接注入後端服務並在畫面端跑工具呼叫迴圈。本專案是 WebAssembly + gRPC-Web，畫面在瀏覽器、後端只在 Server。既有 Weather 範例服務示範了 gRPC-Web 的 unary 與 server-streaming 範式（具體 client + 低階呼叫器，因 WASM 不支援動態 proxy）。RAG 後端（串流對話、語意搜尋）已在 Server 端可解析。

## Goals / Non-Goals

**Goals:**

- 把串流對話經 gRPC-Web 接通到 WASM 前端，畫面與來源儘可能一致。
- 工具呼叫迴圈完整留在 Server，前端只渲染。
- 追問建議以 unary 接通。
- 既有功能零影響。

**Non-Goals:**

- 把 scoped 樣式改寫為 utility 框架（重構階段）。
- 改動既有後端服務邏輯、為依賴外部 API 的服務寫單元測試。
- 既有 Weather 服務變更。

## Decisions

**D1：工具呼叫迴圈留 Server。**
來源把工具委派放畫面端，但委派本質都呼叫 Server 資源（語意搜尋、OpenAI）。在 WASM 放前端等於把每次工具執行拆成額外往返，違背既有「server 端對話狀態串接」設計。Server 新建聊天服務組裝工具、跑既有串流方法（一字不改），前端只收事件。替代方案（迴圈留前端、雙向串流往返）極複雜，捨棄。

**D2：扁平訊息 + 類別列舉，取代多型事件。**
來源串流事件是抽象記錄的繼承階層，protobuf-net 多型支援有限（需特殊標註）。扁平結構 + 列舉在 gRPC-Web 跨瀏覽器最穩，且與既有扁平資料模型一致。回合 id 由回呼改為一類串流事件回傳——gRPC 串流無 out 參數，此為等價妥協。

**D3：追問建議走獨立 unary。**
它本就是一次性、無狀態、無工具的呼叫，與主串流分離最清楚，對應既有 unary 範式。脈絡縮減留前端（純整形，前端有完整訊息清單），使後端建議方法輸入與來源一致、不改。

**D4：UI 連 scoped CSS 原樣搬，兩風格並存。**
目標是畫面儘可能一致；改寫 CSS 必引入視覺偏差且工作量大、有 `::deep`／自訂 web component／檢視器等難以 utility 表達之處。原樣搬最低風險，符合既有「複雜情形用 scoped CSS」的允許層級。Tailwind 化列為後續獨立重構。

**D5：第三方 lib 全搬，但不搬來源 CSS reset。**
Markdown 渲染／消毒／檢視器是畫面全功能所需，一併搬入前端靜態資源。來源的 Tailwind reset 不搬——本專案已有自己的 Tailwind v4 preflight，重複會衝突。

## Risks / Trade-offs

- **Tailwind preflight 改變 Markdown 預設樣式** → 本專案 preflight 把標題大小 reset，可能讓渲染後的 Markdown 走樣。Mitigation：實際跑後依落差，必要時於 scoped CSS 補回標題樣式。
- **兩種 CSS 風格並存造成維護負擔** → 已知取捨，換取快速可運作；重構階段收斂。
- **回合狀態以串流事件回傳，前端漏接則多回合斷裂** → Mitigation：前端務必在每次串流結束前處理該事件並存下。
- **大型靜態資產（PDF 檢視器引擎）增加前端體積** → 已知取捨，換取 citation 全功能。

## Migration Plan

1. Contracts 定義聊天服務介面與扁平資料模型。
2. Server 實作聊天服務（組裝工具、映射事件）、Program 註冊。
3. Client 具體 client + DI。
4. Client 搬入 UI 元件、訊息模型、JS、第三方 lib。
5. 前端聊天頁改注入 gRPC client、消費串流。
6. 編譯、啟動、端到端 SIT。

回退：純新增（合約、服務、client、元件、靜態資源），不動既有資產，回退即移除新增項。

## Open Questions

- 聊天頁路由：掛根路由（取代首頁）或獨立路由。
- 全域共用樣式落點：前端靜態 CSS 或併入 Server 的 Tailwind 來源（後者經 build）。
- Tailwind preflight 對 Markdown 渲染的實際影響，需執行後確認。
