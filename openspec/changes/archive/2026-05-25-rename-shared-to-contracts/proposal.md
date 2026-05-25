## Why

`BlazorN10WasmLab.Shared` 專案承載 gRPC 服務契約（interface + DTO），但 `Shared` 這個名稱在 Blazor 慣例中也用於前端共享 UI 元件資料夾（`BlazorN10WasmLab.Client/Shared/`），造成認知衝突。「Contracts」更精準表達「跨專案合約交換」的本質，且不綁定 protobuf 實作技術，符合 .NET gRPC/WCF 生態慣例。

## What Changes

- **BREAKING**：`BlazorN10WasmLab.Shared` 專案改名為 `BlazorN10WasmLab.Contracts`（csproj、資料夾、assembly name、namespace 全部同步）
- 扁平化 `Contracts/` 子資料夾：`IWeatherService.cs`、`WeatherForecast.cs`、`WeatherForecastReply.cs` 從 `Contracts/` 子資料夾上移至專案根目錄，namespace 由 `BlazorN10WasmLab.Shared.Contracts` 改為 `BlazorN10WasmLab.Contracts`
- 保留 `Surrogates/` 子資料夾，namespace 由 `BlazorN10WasmLab.Shared.Surrogates` 改為 `BlazorN10WasmLab.Contracts.Surrogates`
- 更新 slnx、所有 `ProjectReference`、所有 `using` 陳述
- 更新 `CLAUDE.md` 與當前 spec `openspec/specs/grpc-web-communication/spec.md` 內的專案名稱引用
- archive 內歷史 change 文件**不修改**（保持歷史快照真實性）

## Capabilities

### New Capabilities
（無）

### Modified Capabilities
- `grpc-web-communication`：合約專案名稱從 `BlazorN10WasmLab.Shared` 改為 `BlazorN10WasmLab.Contracts`（requirement 文字描述更新，行為不變）

## 目錄結構對照

**改名前：**
```
BlazorN10WasmLab.Shared\
├── BlazorN10WasmLab.Shared.csproj
├── GrpcTypeModelSetup.cs              (namespace BlazorN10WasmLab.Shared)
├── Contracts\
│   ├── IWeatherService.cs             (namespace BlazorN10WasmLab.Shared.Contracts)
│   ├── WeatherForecast.cs             (namespace BlazorN10WasmLab.Shared.Contracts)
│   └── WeatherForecastReply.cs        (namespace BlazorN10WasmLab.Shared.Contracts)
└── Surrogates\
    └── DateOnlySurrogate.cs           (namespace BlazorN10WasmLab.Shared.Surrogates)
```

**改名後：**
```
BlazorN10WasmLab.Contracts\
├── BlazorN10WasmLab.Contracts.csproj
├── GrpcTypeModelSetup.cs              (namespace BlazorN10WasmLab.Contracts)
├── IWeatherService.cs                 (namespace BlazorN10WasmLab.Contracts)        ← 從 Contracts\ 上移
├── WeatherForecast.cs                 (namespace BlazorN10WasmLab.Contracts)        ← 從 Contracts\ 上移
├── WeatherForecastReply.cs            (namespace BlazorN10WasmLab.Contracts)        ← 從 Contracts\ 上移
└── Surrogates\
    └── DateOnlySurrogate.cs           (namespace BlazorN10WasmLab.Contracts.Surrogates)
```

## Impact

- **csproj / 方案**：5 個 csproj 中的 3 個有 `ProjectReference` 需更新（Client、Server、Shared 本身）；1 個 slnx 需更新
- **原始碼**：7 個 .cs/.razor 檔案的 `using` 需更新（Client/Program.cs、Server/Program.cs、Server/Services/WeatherService.cs、Client/Services/WeatherServiceClient.cs、Client/Pages/Weather.razor、Shared/GrpcTypeModelSetup.cs、加 Surrogates/DateOnlySurrogate.cs）
- **檔案搬移**：5 個 .cs 檔（3 個從 `Contracts/` 上移、2 個維持位置但改 namespace）
- **文件**：`CLAUDE.md` 多處引用、當前 spec `grpc-web-communication/spec.md` 1 處 requirement
- **無 API/行為變更**：純命名重構，wire format、執行行為完全不變
- **無套件相依變更**
