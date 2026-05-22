## Context

BlazorN10WasmLab 是 .NET 10 Blazor Web App（全 InteractiveWebAssembly 模式），目前 CSS 框架為 Bootstrap 5，靜態檔案直接放在 `wwwroot/lib/bootstrap/`。專案沒有 Node.js 建置流程，`.razor` 頁面與元件全部在 `BlazorN10WasmLab.Client` 專案中，`wwwroot` 與入口 `App.razor` 在 server 專案 `BlazorN10WasmLab`。

## Goals / Non-Goals

**Goals:**
- 以 Tailwind CSS v4 完全取代 Bootstrap
- 整合 Tailwind standalone CLI 至 MSBuild，`dotnet build` 自動產生 CSS
- 不引入 Node.js / npm 依賴

**Non-Goals:**
- 不實作 Tailwind watch mode（開發時每次 build 即可）
- 不導入 DaisyUI 或其他 Tailwind plugin（MVP）
- 不改變頁面功能或路由

## Decisions

### 1. 使用 Tailwind v4 standalone CLI，不用 npm

**決策**：下載 `tailwindcss-windows-x64.exe` 放至 solution root `.tools/tailwindcss.exe`，加入 `.gitignore`，在 `CLAUDE.md` 記錄下載指令。

**理由**：專案沒有 Node.js 建置流程，standalone CLI 是最低侵入性的做法。v4 standalone CLI 是單一執行檔，不需要 `node_modules`。

**備選方案**：npm + `package.json` — 需要安裝 Node.js，對此專案而言是額外依賴。

---

### 2. MSBuild target 整合在 server 專案 `.csproj`

**決策**：在 `BlazorN10WasmLab.csproj` 加入 `<Target Name="TailwindBuild" BeforeTargets="Build">`，執行：

```xml
<Exec Command="$(MSBuildThisFileDirectory)..\..\..\.tools\tailwindcss.exe -i Styles\app.css -o wwwroot\app.css" />
```

Release build 加上 `--minify` flag。

**理由**：`wwwroot` 在 server 專案，output CSS 需要在這裡，build target 放在同一個 `.csproj` 最直接。

---

### 3. Tailwind input CSS 放在 `BlazorN10WasmLab/Styles/app.css`

**決策**：
```css
@import "tailwindcss";

@source "../Components/**/*.razor";
@source "../../BlazorN10WasmLab.Client/**/*.razor";
```

原 `wwwroot/app.css` 的 Blazor 特定樣式（`.blazor-error-boundary`、`.valid.modified` 等）也移入此檔，置於 `@import` 之後。

**理由**：將 source 與 output 分離；`Styles/` 是 Tailwind input，`wwwroot/app.css` 是 build output（不手動編輯）。

---

### 4. 元件樣式策略：保留 scoped CSS，Bootstrap class 換成 Tailwind

**決策**：`NavMenu.razor.css` 與 `MainLayout.razor.css` 保留 scoped CSS 機制（Blazor 原生支援），但將其中 Bootstrap utility class 移除，改用 Tailwind utility 或直接在 scoped CSS 寫 CSS。`NavMenu.razor` 的 `navbar-*`、`nav-*` class 改用 Tailwind class。

**理由**：Blazor scoped CSS 本身與 Tailwind 不衝突，保留可避免 `::deep` selector 等複雜重寫。

## Risks / Trade-offs

- **`@source` 路徑解析**：Tailwind v4 standalone CLI 的 `@source` 路徑相對於 input CSS 檔案位置，需確認 glob pattern 正確掃描到兩個專案的 `.razor` 檔案。→ 可在 build 後檢查 output CSS 是否包含預期 class。

- **Tailwind CLI 執行檔不在 git**：新開發者需手動下載。→ 在 `CLAUDE.md` 補充下載指令，並在 MSBuild target 加 `Condition` 檢查檔案是否存在，不存在時印出友善錯誤訊息。

- **Bootstrap 移除後版面跑版**：NavMenu 與 MainLayout 依賴 Bootstrap 的 `.navbar`、`.container-fluid` 等 class。→ 遷移過程中需同步更新 `.razor` 與 scoped CSS。

## Migration Plan

1. 建立 `.tools/` 目錄，下載 `tailwindcss.exe`，加入 `.gitignore`
2. 建立 `Styles/app.css`（Tailwind input）
3. 修改 `BlazorN10WasmLab.csproj`，加入 MSBuild target
4. 執行 `dotnet build`，確認 `wwwroot/app.css` 由 Tailwind 產生
5. 修改 `App.razor`，移除 Bootstrap `<link>`
6. 重寫 `NavMenu.razor` 與 `MainLayout.razor` 的 Bootstrap class
7. 刪除 `wwwroot/lib/bootstrap/` 目錄
8. 執行 app，確認版面正常

**Rollback**：git revert 即可恢復 Bootstrap，Bootstrap 靜態檔案已在 git history 中。
