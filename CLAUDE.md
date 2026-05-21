# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Blazor Web App targeting .NET 10 with WebAssembly (WASM) render mode, orchestrated by .NET Aspire.

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

Four projects with distinct responsibilities:

| Project | Role |
|---|---|
| `BlazorN10WasmLab` | ASP.NET Core server host — serves the app, handles SSR pre-rendering |
| `BlazorN10WasmLab.Client` | Blazor WebAssembly client — all pages and UI components run here in-browser |
| `BlazorN10WasmLab.AppHost` | .NET Aspire orchestrator — manages startup, dashboard, service discovery |
| `BlazorN10WasmLab.ServiceDefaults` | Shared Aspire configuration — OpenTelemetry, health checks (`/health`, `/alive`), HTTP resilience |

### Render Mode

The entire app uses **`InteractiveWebAssembly`** render mode globally (set in `App.razor` on both `<HeadOutlet>` and `<Routes>`). All pages in `BlazorN10WasmLab.Client` run as WASM in the browser — there is no server-side interactive rendering.

### Key Wiring Points

- `BlazorN10WasmLab/Program.cs` — configures `AddRazorComponents().AddInteractiveWebAssemblyComponents()` and maps the Client assembly via `AddAdditionalAssemblies`
- `BlazorN10WasmLab.Client/Program.cs` — minimal WASM bootstrap, add client-side DI services here
- `App.razor` — root HTML shell, loads Bootstrap + app CSS
- `BlazorN10WasmLab.Client/Routes.razor` — client-side router

New pages go in `BlazorN10WasmLab.Client/Pages/`. New services shared across client and server should go in `BlazorN10WasmLab.Client` (or a new shared library) since the Client runs in WASM — it cannot reference server-only assemblies.
