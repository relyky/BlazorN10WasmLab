# chat-ui Specification

## Purpose
TBD - created by archiving change chat-ui-grpc. Update Purpose after archive.
## Requirements
### Requirement: 串流對話於 Server 端編排

聊天 gRPC 服務 SHALL 在 Server 端執行完整的工具呼叫迴圈（載入文件、語意搜尋），呼叫既有串流對話能力，並把產生的事件以 server-streaming 串流給前端。前端 SHALL NOT 直接持有 OpenAI 金鑰或執行工具。

#### Scenario: 送出訊息取得串流回應

- **WHEN** 前端送出一則使用者輸入
- **THEN** Server 執行對話與必要的工具呼叫，逐步串流回逐字文字事件，前端據以即時渲染

#### Scenario: 工具呼叫提示串流給前端

- **WHEN** 模型在回應中發起載入文件或語意搜尋
- **THEN** 串流中出現對應的工具呼叫提示事件，搜尋事件攜帶搜尋關鍵詞與選用的檔名過濾

### Requirement: 多回合對話狀態串接

聊天服務 SHALL 以串流事件回傳本回合的對話狀態識別；前端 SHALL 存下並於下一回合請求帶回，以串接多回合對話。New chat SHALL 清除此狀態。

#### Scenario: 連續兩回合對話

- **WHEN** 使用者在同一對話送出第二則訊息
- **THEN** 請求帶上一回合的狀態識別，助理回應延續先前脈絡

#### Scenario: New chat 重置

- **WHEN** 使用者按 New chat
- **THEN** 對話清空、狀態識別清除，下一則訊息以全新對話開始

### Requirement: 引用呈現與檢視

助理回應中的引用 SHALL 呈現為引用卡片（檔名 + 摘錄）。點擊引用卡片 SHALL 開啟對應的文件檢視器（Markdown 或 PDF）並定位至引用片段。

#### Scenario: 顯示引用卡片

- **WHEN** 助理回應包含引用
- **THEN** 回應末尾渲染引用卡片，顯示來源檔名與摘錄

#### Scenario: 點擊引用開啟檢視器

- **WHEN** 使用者點擊引用卡片
- **THEN** 開啟對應檔型的檢視器並跳至引用片段

### Requirement: 追問建議

一回合對話結束後，前端 SHALL 透過 unary 呼叫取得最多三條追問建議並呈現為按鈕；點擊建議 SHALL 以該建議作為新訊息送出。

#### Scenario: 呈現並點選建議

- **WHEN** 一回合回應結束
- **THEN** 顯示最多三條追問建議按鈕；點擊其一即以該文字送出新訊息

### Requirement: 前端以具體 client 相容 WASM

前端 SHALL 以具體 client 類別（非動態 proxy）呼叫聊天服務，沿用既有 Weather client 的低階呼叫器範式，以相容 WebAssembly 沙盒限制。

#### Scenario: WASM 下成功呼叫

- **WHEN** 前端在瀏覽器發動聊天串流或建議呼叫
- **THEN** 呼叫成功，不因動態 proxy 限制而失敗

### Requirement: 既有功能不受影響

本變更 SHALL NOT 更動既有 Weather 範例服務、其 gRPC-Web 接線、Contracts、Client，或已搬入的後端服務邏輯。

#### Scenario: 既有 SIT 仍通過

- **WHEN** 移轉完成後執行既有 Weather 與 Counter 流程
- **THEN** 兩者行為與移轉前一致

