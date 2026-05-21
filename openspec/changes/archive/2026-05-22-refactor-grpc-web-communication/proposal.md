## Why

目前 Weather 頁面的資料完全在 WASM 客戶端本地產生，沒有真正的 Client-Server 通訊。本次變更建立整個專案的通訊範本：以 gRPC-Web（code-first，protobuf-net.Grpc）取代原本預期的 JSON Web API，獲得型別安全、binary 序列化效能，以及 Server Streaming 能力。

## What Changes

- **新增** `BlazorN10WasmLab.Shared` 專案，存放 gRPC 服務合約（C# interface + ProtoContract 資料模型）
- **新增** `DateOnlySurrogate`，解決 protobuf-net 不支援 `DateOnly` 的問題
- **新增** `WeatherService` 實作於 Server 端，提供 Unary 與 Server Streaming 兩種方法
- **修改** Server `Program.cs`，加入 `AddCodeFirstGrpc`、`UseGrpcWeb`、`MapGrpcService`
- **修改** Client `Program.cs`，注冊 `GrpcChannel`（GrpcWebText 模式）與 `IWeatherService` typed client
- **修改** `Weather.razor`，改由 `IWeatherService` 取得資料（示範 Unary + Streaming 切換）

## Capabilities

### New Capabilities

- `grpc-web-communication`: Client（Blazor WASM）與 Server（ASP.NET Core）之間透過 gRPC-Web 協定通訊的完整基礎設施，包含合約共享、序列化設定、DI 注入模式，作為未來所有 gRPC 服務的範本

### Modified Capabilities

<!-- 無現有 spec 需要修改 -->

## Impact

- **新增專案**：`BlazorN10WasmLab.Shared`（net10.0，Microsoft.NET.Sdk）
- **新增 NuGet 套件**：
  - Shared：`protobuf-net.Grpc`
  - Server：`Grpc.AspNetCore`、`Grpc.AspNetCore.Web`、`protobuf-net.Grpc.AspNetCore`
  - Client：`Grpc.Net.Client`、`Grpc.Net.Client.Web`、`protobuf-net.Grpc`
- **修改檔案**：Server `Program.cs`、Client `Program.cs`、`Weather.razor`
- **新增至 solution**：`BlazorN10WasmLab.Shared.csproj` 須加入 `BlazorN10WasmLab.slnx`
- **不影響**：Aspire AppHost、ServiceDefaults、路由、既有 UI 佈局
