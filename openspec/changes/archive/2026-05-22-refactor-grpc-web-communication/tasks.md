## 1. 建立 Shared 專案與 Solution 設定

- [x] 1.1 新增 `BlazorN10WasmLab.Shared` 類別庫專案（`Microsoft.NET.Sdk`，`net10.0`）
- [x] 1.2 將 Shared 專案加入 `BlazorN10WasmLab.slnx`
- [x] 1.3 安裝 Shared 套件：`protobuf-net.Grpc`

## 2. 實作 Shared 合約

- [x] 2.1 建立 `Surrogates/DateOnlySurrogate.cs`（`[ProtoContract]` struct，DayNumber 橋接）
- [x] 2.2 建立 `Contracts/WeatherForecast.cs`（`[ProtoContract]`，含 Date/TemperatureC/Summary/TemperatureF）
- [x] 2.3 建立 `Contracts/WeatherForecastReply.cs`（`[ProtoContract]`，包含 `List<WeatherForecast>`）
- [x] 2.4 建立 `Contracts/IWeatherService.cs`（`[ServiceContract]`，Unary + Server Streaming 兩個方法）
- [x] 2.5 建立 `GrpcTypeModelSetup.cs`（靜態 `Register()` 方法，初始化 DateOnly surrogate）

## 3. Server 端實作

- [x] 3.1 Server csproj 加入 `ProjectReference` 至 Shared 專案
- [x] 3.2 安裝 Server 套件：`Grpc.AspNetCore`、`Grpc.AspNetCore.Web`、`protobuf-net.Grpc.AspNetCore`
- [x] 3.3 建立 `Services/WeatherService.cs`，實作 `IWeatherService`（Unary 回傳 5 筆，Streaming 逐筆 yield 並延遲 300ms）
- [x] 3.4 修改 Server `Program.cs`：呼叫 `GrpcTypeModelSetup.Register()`
- [x] 3.5 修改 Server `Program.cs`：加入 `builder.Services.AddCodeFirstGrpc()`
- [x] 3.6 修改 Server `Program.cs`：加入 `app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true })`
- [x] 3.7 修改 Server `Program.cs`：加入 `app.MapGrpcService<WeatherService>().EnableGrpcWeb()`

## 4. Client 端實作

- [x] 4.1 Client csproj 加入 `ProjectReference` 至 Shared 專案
- [x] 4.2 安裝 Client 套件：`Grpc.Net.Client`、`Grpc.Net.Client.Web`、`protobuf-net.Grpc`
- [x] 4.3 修改 Client `Program.cs`：呼叫 `GrpcTypeModelSetup.Register()`
- [x] 4.4 修改 Client `Program.cs`：建立 `GrpcChannel`（`GrpcWebMode.GrpcWebText`，`BaseAddress`）
- [x] 4.5 修改 Client `Program.cs`：`builder.Services.AddSingleton(channel.CreateGrpcService<IWeatherService>())`

## 5. Weather 頁面改寫

- [x] 5.1 修改 `Weather.razor`：加入 `@inject IWeatherService WeatherSvc`
- [x] 5.2 修改 `Weather.razor`：實作「刷新（Unary）」按鈕，呼叫 `GetForecastsAsync()`
- [x] 5.3 修改 `Weather.razor`：實作「刷新（Streaming）」按鈕，呼叫 `StreamForecastsAsync()` 逐筆更新 UI
- [x] 5.4 移除舊有的本地假資料產生邏輯（`Enumerable.Range` + `Random.Shared`）

## 6. 驗證

- [x] 6.1 執行 `dotnet build BlazorN10WasmLab.slnx`，確認全方案 build 成功
- [x] 6.2 啟動應用程式，驗證 Unary 模式可正常取得並顯示天氣資料
- [x] 6.3 啟動應用程式，驗證 Streaming 模式可逐筆顯示天氣資料（有延遲間隔）
- [x] 6.4 確認 `DateOnly` 日期值顯示正確（無時區偏移）
