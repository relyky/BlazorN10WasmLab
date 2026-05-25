# BlazorN10WasmLab

一個 **Blazor WebAssembly + .NET 10 + .NET Aspire** 的實驗場，把幾個現代化但組合起來容易踩坑的技術串在一起，邊玩邊累積最佳實踐。

## 技術棧

- **.NET 10** Blazor Web App，全域 `InteractiveWebAssembly` render mode（整個前端在瀏覽器跑）
- **.NET Aspire** 統一啟動 server、dashboard、service discovery
- **gRPC-Web + protobuf-net.Grpc** code-first 通訊（含 WASM 的 `Reflection.Emit` 繞道與 `DateOnly` surrogate）
- **Tailwind CSS v4** standalone CLI（無 npm），整合至 MSBuild
- **OpenSpec** 規格驅動的變更管理流程

## 特色

| 主題 | 重點 |
|---|---|
| WASM gRPC-Web | 不能用 `CreateGrpcService<T>()`（瀏覽器沒 Reflection.Emit），改寫具體 client 直接呼叫 `CallInvoker` |
| Tailwind v4 | 透過 MSBuild target 自動 build；individual transform property（`translate`、`rotate`、`scale`）的 transition 寫法地雷已記錄 |
| /launch skill | 並行 `dotnet watch` + Tailwind watch，補上 Hot Reload 跳過 MSBuild 導致 Tailwind 不更新的落差 |
| OpenSpec 工作流 | 每次功能/修正都有 proposal、design、specs delta、tasks，archive 後 spec 自動同步 |

## 快速啟動

需要 **.NET 10 SDK** 與 **PowerShell 7+**（Windows）。

```powershell
# 1. 下載 Tailwind CLI（首次設定）
Invoke-WebRequest `
  -Uri "https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-windows-x64.exe" `
  -OutFile ".tools\tailwindcss.exe"

# 2. 還原套件 + 建置
dotnet restore BlazorN10WasmLab.slnx
dotnet build BlazorN10WasmLab.slnx

# 3. 以 Aspire 啟動（含 Dashboard）
dotnet run --project BlazorN10WasmLab.AppHost
```

開發 URL：
- `https://blazorn10wasmlab.dev.localhost:7009`（直跑 server）
- Aspire Dashboard 啟動時會自動開啟

## 專案結構

```
BlazorN10WasmLab/              ASP.NET Core 主機（serve app + gRPC-Web）
BlazorN10WasmLab.Client/       Blazor WebAssembly 客戶端（頁面、UI、gRPC client）
BlazorN10WasmLab.Contracts/    gRPC 合約庫（ServiceContract / 資料模型 / protobuf 設定）
BlazorN10WasmLab.AppHost/      .NET Aspire 啟動編排
BlazorN10WasmLab.ServiceDefaults/  共用 Aspire 設定（OpenTelemetry / 健康檢查 / HTTP resilience）

openspec/                      規格與變更管理
.claude/skills/                Claude Code 自訂 skills（launch、sit-counter…）
```

## 文件指引

- **`CLAUDE.md`** — Claude Code 的工作守則，含 gRPC-Web 命名規則、Tailwind 慣例、踩過的雷。人類開發者也建議讀。
- **`openspec/specs/`** — 當前每個能力的 requirement 規格
- **`openspec/changes/archive/`** — 過去所有變更的提案、設計、實作 tasks，可追溯每個決策的脈絡

## License

未授權，內部實驗用途。
