## Context

專案為 Blazor Web App（.NET 10，WASM render mode），目前 Weather 頁面在 WASM 客戶端本地產生假資料，沒有任何 Client-Server 通訊。本次設計建立以 gRPC-Web 為基礎的通訊範本，供未來所有 Client-Server 服務使用。

瀏覽器環境中的 Blazor WASM 不支援標準 gRPC（HTTP/2 trailer 限制），必須使用 gRPC-Web 協定作為橋接。

## Goals / Non-Goals

**Goals:**
- 建立可重複使用的 gRPC-Web 通訊基礎設施
- 以 Weather 功能示範 Unary（一問一答）與 Server Streaming 兩種模式
- 型別安全：Client 與 Server 共享同一份 C# 合約（interface + data model）
- 解決 `DateOnly` 的 protobuf 序列化問題

**Non-Goals:**
- 不實作 Client Streaming 或 Bidirectional Streaming（超出 Lab 示範範疇）
- 不處理 gRPC 認證/授權
- 不建立 gRPC 錯誤處理框架（使用預設 StatusCode 即可）
- 不移除 Aspire 或修改服務探索設定

## Decisions

### D1：code-first（protobuf-net.Grpc）vs proto-first（Grpc.Tools）

**選擇**：code-first with `protobuf-net.Grpc`

**理由**：
- 合約直接用 C# interface 定義，不需要 `.proto` 語法與額外 codegen 步驟
- `[ServiceContract]` / `[ProtoContract]` 對 .NET 開發者直覺易懂
- Shared 專案可直接被 Server 與 Client 參照，無需 generated stub 的 build pipeline

**放棄 proto-first 的原因**：需要 `protobuf-net.Grpc` 與 `Grpc.Tools` 無法混用，且 Lab 環境不需要跨語言互通性。

---

### D2：Shared 專案位置

**選擇**：新增 `BlazorN10WasmLab.Shared`（`Microsoft.NET.Sdk`，`net10.0`）

```
BlazorN10WasmLab.Shared/
├── Contracts/
│   ├── IWeatherService.cs
│   ├── WeatherForecast.cs
│   └── WeatherForecastReply.cs
└── Surrogates/
    └── DateOnlySurrogate.cs
```

Server 和 Client 都 `ProjectReference` 此專案。合約一處定義，兩端共用。

---

### D3：DateOnly 序列化策略

**選擇**：路B — protobuf-net surrogate 轉換器

**理由**：保留 `DateOnly` 型別在合約介面與 UI 層，不影響呼叫方的使用體驗。

實作方式：
```csharp
[ProtoContract]
public struct DateOnlySurrogate
{
    [ProtoMember(1)] public int DayNumber { get; set; }
    public static implicit operator DateOnly(DateOnlySurrogate s) 
        => DateOnly.FromDayNumber(s.DayNumber);
    public static implicit operator DateOnlySurrogate(DateOnly d) 
        => new() { DayNumber = d.DayNumber };
}
```

Server 與 Client 的 `Program.cs` 啟動時各自呼叫一次：
```csharp
RuntimeTypeModel.Default
    .Add(typeof(DateOnly), false)
    .SetSurrogate(typeof(DateOnlySurrogate));
```

---

### D4：gRPC-Web 模式

**選擇**：`GrpcWebMode.GrpcWebText`（base64 編碼）

**理由**：相容性最佳，Lab 環境不追求極致效能。`GrpcWebMode.GrpcWeb`（binary）在部分瀏覽器的 Server Streaming 情境下有已知相容問題。

---

### D5：Client DI 注入模式

**選擇**：Option A — 直接注入 typed interface

```csharp
// Client Program.cs
var channel = GrpcChannel.ForAddress(
    builder.HostEnvironment.BaseAddress,
    new GrpcChannelOptions
    {
        HttpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler())
    });

builder.Services.AddSingleton<IWeatherService>(new WeatherServiceClient(channel));
```

頁面直接 `@inject IWeatherService WeatherSvc`，符合 Blazor DI 慣例，未來每個新服務獨立注冊即可。

> **實作偏差**：原計畫使用 `channel.CreateGrpcService<IWeatherService>()`，但 Blazor WASM 瀏覽器沙盒不支援 `System.Reflection.Emit`，導致 `DefaultProxyCache<T>` 初始化失敗。改為在 `Client/Services/WeatherServiceClient.cs` 建立具體實作類別，直接使用 `CallInvoker` 與手動定義的 gRPC method descriptors，完全繞過動態 proxy 生成。DI 介面（`IWeatherService`）不變，呼叫端無感知。

---

### D6：Server Streaming 示範方式

`IWeatherService` 同時定義 Unary 與 Streaming 兩個方法：

```csharp
[ServiceContract]
public interface IWeatherService
{
    ValueTask<WeatherForecastReply> GetForecastsAsync(
        CallContext context = default);
    
    IAsyncEnumerable<WeatherForecast> StreamForecastsAsync(
        CallContext context = default);
}
```

`Weather.razor` 提供切換按鈕，讓使用者可觀察兩種模式的行為差異。

## Risks / Trade-offs

| 風險 | 緩解措施 |
|------|----------|
| protobuf-net.Grpc 與 Grpc.AspNetCore 版本不相容 | 鎖定 stable 版本；build 失敗時優先檢查版本矩陣 |
| DateOnly surrogate 未在啟動前初始化導致序列化錯誤 | 在 Shared 提供靜態 `GrpcTypeModelSetup.Register()` 方法，兩端各呼叫一次 |
| GrpcWebText base64 增加約 33% payload 大小 | Lab 示範可接受；Production 需求改用 GrpcWeb binary |
| Server Streaming 在 GrpcWebText 模式下需保持連線 | 示範用途，不處理斷線重連 |

## Migration Plan

1. 新增 Shared 專案並加入 solution
2. 實作 Server 端（套件、Service、Program.cs）→ 確認 gRPC endpoint 可用
3. 實作 Client 端（套件、Program.cs、Weather.razor）→ 端對端測試
4. 無 rollback 需求（Lab 環境，無既有 API consumers）

## Open Questions

- 無。所有決策已在 explore 階段確認。
