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

### Claude Code 快速啟動／停止

開發時優先用 `/launch` 而非手動 `dotnet run`：

| 指令 | 說明 |
|---|---|
| `/launch start` | 並行啟動 Tailwind CLI watch 與 `dotnet watch` (Aspire AppHost, Hot Reload)，自動開啟瀏覽器至首頁並截圖確認 |
| `/launch stop` | 終止兩個背景 watch 程序、清除 session 暫存檔 |
| `/launch`（無 args）| 自動偵測：未啟動則 start，已啟動則詢問是否 stop |

`/launch start` 解決一個關鍵問題：`dotnet watch` 的 Hot Reload 路徑跳過 MSBuild，導致綁在 MSBuild target 上的 Tailwind CLI 不會重跑（新增 utility class 不會出現在 `wwwroot/app.css`）。並行 Tailwind watch 即時重生 CSS 補上這個落差。

Session task ID 暫存於 `.claude/aspire-debug_session.tmp` 與 `.claude/tailwind-watch_session.tmp`；log 寫入 `.claude/tmp/{aspire-startup,tailwind-watch}.log`。

**Development URLs** (direct run, no Aspire):
- HTTP: `http://blazorn10wasmlab.dev.localhost:5158`
- HTTPS: `https://blazorn10wasmlab.dev.localhost:7009`

## Architecture

五個專案各有明確職責：

| Project | Role |
|---|---|
| `BlazorN10WasmLab` | ASP.NET Core server host — serves the app, gRPC-Web endpoints |
| `BlazorN10WasmLab.Client` | Blazor WebAssembly client — all pages and UI components run here in-browser |
| `BlazorN10WasmLab.Contracts` | gRPC 合約庫 — service interfaces, data models, protobuf setup |
| `BlazorN10WasmLab.AppHost` | .NET Aspire orchestrator — manages startup, dashboard, service discovery |
| `BlazorN10WasmLab.ServiceDefaults` | Shared Aspire configuration — OpenTelemetry, health checks (`/health`, `/alive`), HTTP resilience |

### Render Mode

The entire app uses **`InteractiveWebAssembly`** render mode globally (set in `App.razor`). All pages in `BlazorN10WasmLab.Client` run as WASM in the browser.

### Key Wiring Points

- `BlazorN10WasmLab/Program.cs` — gRPC-Web 設定（`AddCodeFirstGrpc`、`UseGrpcWeb`、`MapGrpcService`）、`GrpcTypeModelSetup.Register()`
- `BlazorN10WasmLab.Client/Program.cs` — WASM bootstrap，`GrpcChannel` 建立與所有 gRPC service DI 注冊
- `BlazorN10WasmLab.Contracts/GrpcTypeModelSetup.cs` — `DateOnly` surrogate 初始化，Server 與 Client 啟動時各呼叫一次

## gRPC-Web 通訊規則

### 新增 gRPC Service 的流程

1. **Contracts 專案**新增 interface（`[ServiceContract]`）與資料模型（`[ProtoContract]`）
2. **Server 專案**實作 interface，在 `Program.cs` 加 `app.MapGrpcService<T>().EnableGrpcWeb()`
3. **Client 專案**新增具體 client 類別，在 `Program.cs` 注冊 `AddSingleton<IXxxService>(new XxxServiceClient(channel))`

### WASM 不能用 `CreateGrpcService<T>()`

`channel.CreateGrpcService<T>()` 在 Blazor WASM 執行時會拋出 `TypeInitializationException`，因為 `DefaultProxyCache<T>` 使用 `System.Reflection.Emit`，在瀏覽器沙盒不可用。

**必須**在 `BlazorN10WasmLab.Client/Services/` 建立具體實作類別（參考 `WeatherServiceClient.cs`），直接使用 `CallInvoker`。

### protobuf-net.Grpc 命名規則（Client 端 method descriptor 需對應）

- **Service name**：`{Namespace}.{TypeName去掉開頭I}` → `IWeatherService` in `BlazorN10WasmLab.Contracts` → `BlazorN10WasmLab.Contracts.WeatherService`
- **Method name**：去掉 `Async` 後綴 → `GetForecastsAsync` → `GetForecasts`

### DateOnly 序列化

`protobuf-net` 原生不支援 `DateOnly`。`GrpcTypeModelSetup.Register()` 透過 surrogate（`DateOnlySurrogate`，存 `DayNumber`）橋接。Server 與 Client 啟動時都必須呼叫，且在任何 gRPC 呼叫前完成。

### GrpcWebText 模式

Client 固定使用 `GrpcWebMode.GrpcWebText`（base64）確保跨瀏覽器相容性。服務間無請求參數的方法使用 `GrpcEmpty`（0-byte protobuf，與 server 端 `ProtoBuf.Grpc.Internal.Empty` wire-compatible）。

`GrpcEmpty` 定義在 `BlazorN10WasmLab.Client/Services/WeatherServiceClient.cs` 底部，新增服務時直接引用同一個型別，無需重複定義。

### Server-side Service 位置

Server 實作類別放在 `BlazorN10WasmLab/Services/` 目錄，namespace 為 `BlazorN10WasmLab.Services`（非 `Components`）。

## Tailwind CSS

CSS 框架使用 **Tailwind CSS v4**，透過 standalone CLI（無 npm）整合至 MSBuild。

### Tailwind CLI 下載（首次設定）

CLI 執行檔不納入 git，需手動下載至 `.tools/tailwindcss.exe`：

```powershell
# 下載 Tailwind CSS v4 standalone CLI (Windows x64)
Invoke-WebRequest -Uri "https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-windows-x64.exe" -OutFile ".tools\tailwindcss.exe"
```

### Tailwind 建置流程

- **Input CSS**：`BlazorN10WasmLab/BlazorN10WasmLab/Styles/app.css`（含 `@import "tailwindcss"` 與 Blazor 特定樣式）
- **Output CSS**：`BlazorN10WasmLab/BlazorN10WasmLab/wwwroot/app.css`（由 MSBuild target 自動產生，不手動編輯）
- `dotnet build` 時自動執行；Release build 加 `--minify`

### CSS 架構（三層）

| 層級 | 位置 | 用途 |
|---|---|---|
| **全域樣式** | `BlazorN10WasmLab/BlazorN10WasmLab/Styles/app.css` | Blazor 特定樣式（`.blazor-error-boundary`、`.valid.modified` 等），勿放一般 UI 樣式 |
| **元件 scoped CSS** | `ComponentName.razor.css` | 無法簡潔用 utility 表達的版面、`::deep` 選擇器、SVG icon 背景、媒體查詢 |
| **Tailwind utility** | 直接寫在 `.razor` HTML 上 | 字體、顏色、間距、顯示模式等 — 優先用這層 |

### 使用慣例

**Tailwind v4 base reset 將所有標題字體大小重設為 `inherit`**，必須明確加 class：

```razor
<h1 class="text-3xl font-bold mb-2">標題</h1>
<h2 class="text-xl font-semibold mb-1">次標題</h2>
<p class="text-base text-gray-500">內文</p>
```

**優先順序原則**：
1. 先用 Tailwind utility class 直接修飾元素
2. 需要 `::deep`、CSS selector 組合、或超過 5 個 utility 時，改用 scoped CSS
3. 只有 Blazor 框架相關樣式才寫進 `Styles/app.css`
