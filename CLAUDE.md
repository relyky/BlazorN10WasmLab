# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Blazor Web App targeting .NET 10 with WebAssembly (WASM) render mode, orchestrated by .NET Aspire. Client-Server 通訊使用 **gRPC-Web（code-first，protobuf-net.Grpc）**。

## Commands

```powershell
# Run via Aspire (recommended — starts app + Aspire Dashboard)
dotnet run --project BlazorN10WasmLab.AppHost

# Run server directly (without Aspire Dashboard)
dotnet run --project BlazorN10WasmLab\BlazorN10WasmLab\BlazorN10WasmLab.csproj

# Build entire solution
dotnet build BlazorN10WasmLab.slnx

# Restore packages
dotnet restore BlazorN10WasmLab.slnx
```

**Development URLs** (direct run, no Aspire):
- HTTP: `http://blazorn10wasmlab.dev.localhost:5158`
- HTTPS: `https://blazorn10wasmlab.dev.localhost:7009`

## Architecture

五個專案各有明確職責：

| Project | Role |
|---|---|
| `BlazorN10WasmLab` | ASP.NET Core server host — serves the app, gRPC-Web endpoints |
| `BlazorN10WasmLab.Client` | Blazor WebAssembly client — all pages and UI components run here in-browser |
| `BlazorN10WasmLab.Shared` | gRPC 合約庫 — service interfaces, data models, protobuf setup |
| `BlazorN10WasmLab.AppHost` | .NET Aspire orchestrator — manages startup, dashboard, service discovery |
| `BlazorN10WasmLab.ServiceDefaults` | Shared Aspire configuration — OpenTelemetry, health checks (`/health`, `/alive`), HTTP resilience |

### Render Mode

The entire app uses **`InteractiveWebAssembly`** render mode globally (set in `App.razor`). All pages in `BlazorN10WasmLab.Client` run as WASM in the browser.

### Key Wiring Points

- `BlazorN10WasmLab/Program.cs` — gRPC-Web 設定（`AddCodeFirstGrpc`、`UseGrpcWeb`、`MapGrpcService`）、`GrpcTypeModelSetup.Register()`
- `BlazorN10WasmLab.Client/Program.cs` — WASM bootstrap，`GrpcChannel` 建立與所有 gRPC service DI 注冊
- `BlazorN10WasmLab.Shared/GrpcTypeModelSetup.cs` — `DateOnly` surrogate 初始化，Server 與 Client 啟動時各呼叫一次

## gRPC-Web 通訊規則

### 新增 gRPC Service 的流程

1. **Shared 專案**新增 interface（`[ServiceContract]`）與資料模型（`[ProtoContract]`）
2. **Server 專案**實作 interface，在 `Program.cs` 加 `app.MapGrpcService<T>().EnableGrpcWeb()`
3. **Client 專案**新增具體 client 類別，在 `Program.cs` 注冊 `AddSingleton<IXxxService>(new XxxServiceClient(channel))`

### WASM 不能用 `CreateGrpcService<T>()`

`channel.CreateGrpcService<T>()` 在 Blazor WASM 執行時會拋出 `TypeInitializationException`，因為 `DefaultProxyCache<T>` 使用 `System.Reflection.Emit`，在瀏覽器沙盒不可用。

**必須**在 `BlazorN10WasmLab.Client/Services/` 建立具體實作類別（參考 `WeatherServiceClient.cs`），直接使用 `CallInvoker`。

### protobuf-net.Grpc 命名規則（Client 端 method descriptor 需對應）

- **Service name**：`{Namespace}.{TypeName去掉開頭I}` → `IWeatherService` in `BlazorN10WasmLab.Shared.Contracts` → `BlazorN10WasmLab.Shared.Contracts.WeatherService`
- **Method name**：去掉 `Async` 後綴 → `GetForecastsAsync` → `GetForecasts`

### DateOnly 序列化

`protobuf-net` 原生不支援 `DateOnly`。`GrpcTypeModelSetup.Register()` 透過 surrogate（`DateOnlySurrogate`，存 `DayNumber`）橋接。Server 與 Client 啟動時都必須呼叫，且在任何 gRPC 呼叫前完成。

### GrpcWebText 模式

Client 固定使用 `GrpcWebMode.GrpcWebText`（base64）確保跨瀏覽器相容性。服務間無請求參數的方法使用 `GrpcEmpty`（0-byte protobuf，與 server 端 `ProtoBuf.Grpc.Internal.Empty` wire-compatible）。
