## 1. 共享元件基礎設施

- [x] 1.1 新增資料夾 `BlazorN10WasmLab.Client/Shared/`
- [x] 1.2 編輯 `BlazorN10WasmLab.Client/_Imports.razor`，於既有 using 區段末尾加入 `@using BlazorN10WasmLab.Client.Shared`

## 2. MaterialInput 元件實作

- [x] 2.1 新建 `BlazorN10WasmLab.Client/Shared/MaterialInput.razor`
- [x] 2.2 於檔案頂部撰寫 razor 註解，註明「未整合 EditContext，在 `<EditForm>` 內請改用 `<InputText>`」
- [x] 2.3 撰寫 markup：`<div class="relative w-full @Class mt-6">` 包覆 `<input>` 與 `<label>`
- [x] 2.4 input 使用 `placeholder=" "`（單一空白）以觸發 `:not(:placeholder-shown)`
- [x] 2.5 input class：`peer w-full bg-transparent text-slate-700 text-sm border-b border-slate-200 focus:border-[var(--color-primary)] pl-0 pr-3 py-2 transition-all duration-300 outline-none`
- [x] 2.6 input 繫結使用 `@bind-value="CurrentValue" @bind-value:event="oninput"` 以即時更新
- [x] 2.7 label class：含 `peer-focus:-top-3.5 peer-focus:text-xs peer-focus:text-[var(--color-primary)]` 與 `peer-[:not(:placeholder-shown)]:-top-3.5 peer-[:not(:placeholder-shown)]:text-xs`
- [x] 2.8 `@code` 區宣告參數：`Id`（`string?`，無預設值）、`Label`（預設 "欄位名稱"）、`Type`（預設 "text"）、`Class`（預設 "max-w-sm"）、`Value`（`string?`）、`ValueChanged`（`EventCallback<string?>`）
- [x] 2.9 `OnInitialized` 內 `Id ??= $"mi-{Guid.NewGuid():N}";` 生成 stable id
- [x] 2.10 私有 `CurrentValue` 屬性：get 回 `Value`，set 呼叫 `ValueChanged.InvokeAsync(value)`

## 3. Playground 頁面

- [x] 3.1 新建 `BlazorN10WasmLab.Client/Pages/Playground.razor`，加 `@page "/playground"` 與 `<PageTitle>Playground</PageTitle>`
- [x] 3.2 h1 使用 `class="text-4xl font-bold text-[var(--color-primary)] my-4"`，顯示 "Playground"
- [x] 3.3 新增區塊一：`<MaterialInput @bind-Value="_name" Label="姓名" />`，下方以 `<p>` 顯示 `目前值：@_name`
- [x] 3.4 新增區塊二：`<MaterialInput @bind-Value="_email" Label="Email" Type="email" />`，下方以 `<p>` 顯示 `目前值：@_email`
- [x] 3.5 `@code` 宣告 `private string? _name;` 與 `private string? _email;`

## 4. NavMenu 連結

- [x] 4.1 編輯 `BlazorN10WasmLab.Client/Layout/NavMenu.razor`，於 Weather NavLink 之後新增 Playground NavLink
- [x] 4.2 Playground NavLink 格式：`<NavLink class="nav-link" href="playground" @onclick="HandleNavClick">` + 16x16 SVG icon（currentColor）+ 文字 "Playground"

## 5. 驗證

- [x] 5.1 確認 `dotnet build BlazorN10WasmLab.slnx` 無 error / warning
- [x] 5.2 `/launch start` 啟動 Aspire + Tailwind watch
- [x] 5.3 確認 `wwwroot/app.css` 內生成 `.focus\:border-\[var\(--color-primary\)\]` 與 `.peer-focus\:text-\[var\(--color-primary\)\]` 兩條 class
- [x] 5.4 瀏覽器導覽至 `/playground`，截圖確認初始畫面（兩個 input、label 在原位）
- [x] 5.5 點擊第一個 input：截圖確認 label 浮動且顏色為 magenta、底線變 magenta
- [x] 5.6 在 input 內打字：截圖確認下方「目前值」即時更新
- [x] 5.7 第一個 input 失焦但留值：確認 label 維持浮動
- [x] 5.8 第一個 input 清空後失焦：確認 label 回到原位
- [x] 5.9 NavMenu 出現 Playground 連結，點擊可導覽且 active 高亮
- [x] 5.10 JS 確認 input 的 `id` 屬性格式為 `mi-{32 hex}`（在未明確傳 Id 的情況下）
