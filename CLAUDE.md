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
| `/launch`（無 args）| 報錯：必須指定 start 或 stop |

`/launch start` 解決一個關鍵問題：`dotnet watch` 的 Hot Reload 路徑跳過 MSBuild，導致綁在 MSBuild target 上的 Tailwind CLI 不會重跑（新增 utility class 不會出現在 `wwwroot/app.css`）。並行 Tailwind watch 即時重生 CSS 補上這個落差。

PID 暫存於 `.claude/tmp/{aspire,tailwind}.pid`；log 寫入 `.claude/tmp/{aspire-startup,tailwind-watch}.log`。

**Development URLs** (direct run, no Aspire):
- HTTP: `http://blazorn10wasmlab.dev.localhost:5158`
- HTTPS: `https://blazorn10wasmlab.dev.localhost:7009`

## Architecture

專案結構、render mode、關鍵接線點見 `docs/architecture.md`。

## gRPC-Web 通訊規則

Client-Server 通訊使用 gRPC-Web（code-first，protobuf-net.Grpc）。新增 gRPC Service 的流程：

1. **Contracts 專案**新增 interface（`[ServiceContract]`）與資料模型（`[ProtoContract]`）
2. **Server 專案**實作 interface，在 `Program.cs` 加 `app.MapGrpcService<T>().EnableGrpcWeb()`
3. **Client 專案**新增具體 client 類別，在 `Program.cs` 注冊 `AddSingleton<IXxxService>(new XxxServiceClient(channel))`

WASM 為什麼不能用 `CreateGrpcService<T>()`、protobuf-net.Grpc 命名規則、DateOnly 序列化、GrpcWebText／`GrpcEmpty`、Server-side service 位置等實作細節與設計緣由，見 `docs/architecture.md`。

## Tailwind CSS

CSS 框架使用 **Tailwind CSS v4**，透過 standalone CLI（無 npm）整合至 MSBuild。撰寫樣式優先用 Tailwind utility class 直接寫在 `.razor`，需要 `::deep`／selector 組合或超過 5 個 utility 時改用 scoped CSS。

CLI 下載、建置流程、CSS 三層架構、使用慣例（標題 reset、優先順序）、Tailwind v4 動畫地雷（`translate-x-*` 不是 `transform`）見 `docs/architecture.md`。

## OpenSpec 工作流

本專案使用 OpenSpec 管理規格與變更：

- `openspec/specs/<capability>/spec.md` — 各能力當前規格（requirements + scenarios）
- `openspec/changes/<name>/` — 進行中的變更（含 proposal/design/specs/tasks）
- `openspec/changes/archive/YYYY-MM-DD-<name>/` — 已完成歸檔

常用斜線指令：`/opsx:propose`（提案）、`/opsx:apply`（實作）、`/opsx:verify`（驗證）、`/opsx:archive`（歸檔同步 spec）。修改既有能力的 requirement 時，delta spec 走 `## MODIFIED Requirements` + 完整貼上整個 requirement 區塊（保留所有 scenarios 並編輯）。

## Agent skills

### Issue tracker

不使用傳統 issue tracker；議題與開發進程透過 OpenSpec 工作流管理，記入 `openspec/` 目錄。See `docs/agents/issue-tracker.md`.

### Triage labels

五個標準角色使用預設字串（needs-triage / needs-info / ready-for-agent / ready-for-human / wontfix）。See `docs/agents/triage-labels.md`.

### Domain docs

Single-context：根目錄 `CONTEXT.md` + `docs/adr/`。See `docs/agents/domain.md`.
