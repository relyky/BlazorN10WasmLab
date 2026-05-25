# Spec: gRPC-Web Communication

### Requirement: Shared gRPC contract project
系統 SHALL 提供一個獨立的 `BlazorN10WasmLab.Contracts` 類別庫專案，存放 gRPC 服務介面與資料模型，同時被 Server 與 Client 專案參照。

#### Scenario: Contracts project referenced by both server and client
- **WHEN** 開發者在 Contracts 專案修改 `IWeatherService` 介面
- **THEN** Server 與 Client 的 build 均能反映該變更（編譯錯誤或型別更新）

---

### Requirement: DateOnly protobuf serialization
系統 SHALL 透過 surrogate 轉換器支援 `DateOnly` 型別的 protobuf 序列化，且 Server 與 Client 啟動時均 MUST 完成初始化。

#### Scenario: DateOnly value round-trips correctly
- **WHEN** Server 回傳含有 `DateOnly` 欄位的 `WeatherForecast`
- **THEN** Client 收到的 `DateOnly` 值與 Server 原始值相同（日期不偏移）

---

### Requirement: gRPC-Web unary call
系統 SHALL 支援從 Blazor WASM 發出 gRPC-Web Unary 呼叫，一次取得完整的天氣預報陣列。

#### Scenario: Client fetches all forecasts at once
- **WHEN** 使用者載入 Weather 頁面或點擊「刷新（Unary）」
- **THEN** 頁面顯示 5 筆天氣預報資料（日期、溫度 C/F、摘要）

#### Scenario: Loading state shown during fetch
- **WHEN** Unary 呼叫進行中
- **THEN** 頁面顯示 Loading 指示器，刷新按鈕不可點擊

---

### Requirement: gRPC-Web server streaming call
系統 SHALL 支援從 Blazor WASM 發出 gRPC-Web Server Streaming 呼叫，逐筆接收天氣預報資料。

#### Scenario: Client receives forecasts one by one
- **WHEN** 使用者點擊「刷新（Streaming）」
- **THEN** 天氣預報資料逐筆出現在表格中（每筆有明顯間隔延遲，示範串流效果）

#### Scenario: Streaming mode uses GrpcWebText
- **WHEN** Client 建立 GrpcChannel
- **THEN** HttpHandler 使用 `GrpcWebMode.GrpcWebText` 確保跨瀏覽器相容性

---

### Requirement: Server gRPC-Web middleware
Server SHALL 啟用 gRPC-Web middleware，並將 WeatherService endpoint 標記為支援 gRPC-Web。

#### Scenario: gRPC-Web endpoint accessible from browser
- **WHEN** WASM Client 從瀏覽器發出 gRPC-Web 請求至 WeatherService
- **THEN** Server 正確處理請求並回傳結果（不因 HTTP/2 trailer 限制而失敗）

---

### Requirement: Typed service DI injection
Client SHALL 透過 DI 直接注入 `IWeatherService` typed client，頁面不需直接操作 `GrpcChannel`。

#### Scenario: Weather page injects IWeatherService
- **WHEN** `Weather.razor` 需要呼叫天氣服務
- **THEN** 可直接使用 `@inject IWeatherService WeatherSvc` 取得 typed client，無需手動建立 channel 或 proxy
