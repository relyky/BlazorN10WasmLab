## MODIFIED Requirements

### Requirement: Shared gRPC contract project
系統 SHALL 提供一個獨立的 `BlazorN10WasmLab.Contracts` 類別庫專案，存放 gRPC 服務介面與資料模型，同時被 Server 與 Client 專案參照。

#### Scenario: Contracts project referenced by both server and client
- **WHEN** 開發者在 Contracts 專案修改 `IWeatherService` 介面
- **THEN** Server 與 Client 的 build 均能反映該變更（編譯錯誤或型別更新）
