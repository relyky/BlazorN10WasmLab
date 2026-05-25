## Context

專案使用 Blazor WebAssembly + Tailwind CSS v4。三個現有頁面都不含表單輸入元素，所有 razor component 散落在 `Pages/` 與 `Layout/`。要開始建立可重用元件庫，需要先確立資料夾／命名空間慣例，並以一個小元件（floating-label input）驗證模式。

主題色已透過 CSS 變數 `--color-primary` (#c2185b magenta) 統一，現有頁面（Home、Counter）皆用 `text-[var(--color-primary)]` 與 `bg-[var(--color-primary)]` arbitrary value 引用。Tailwind 透過 standalone CLI 並行 watch（不走 MSBuild），任何新出現的 utility 字面值需被 CLI 偵測到才會生成至 `wwwroot/app.css`。

## Goals / Non-Goals

**Goals:**
- 建立 `Client/Shared/` 資料夾與 `BlazorN10WasmLab.Client.Shared` namespace 作為共用元件落點
- `MaterialInput` 元件支援 `string?` 雙向繫結、即時更新、Material 風格浮動 label
- 視覺與主題色整合（focus / floating label 用 `var(--color-primary)`）
- 提供 `/playground` 沙盒頁面手動驗證
- 不污染既有 Counter / Weather demo

**Non-Goals:**
- 不整合 `EditContext` / `DataAnnotationsValidator`（EditForm 場景請改用 `<InputText>`）
- 不支援 generic `TValue`（只做 `string?`）
- 不處理中文 IME composition 中途 binding（接受 oninput 行為）
- 不建立 design system / 主題切換機制
- 不建立 Storybook 等正式元件測試框架

## Decisions

### D1：元件命名保留 `MaterialInput`

替換主題色後嚴格說已非 Material 規範，但 Material 概念點是「floating label + 底線 input」這個視覺 pattern，與主題色無關。維持 `MaterialInput` 命名能直觀傳達意圖，未來若新增其他 input 風格再以另一名稱區分（如 `OutlinedInput`、`PlainInput`）。

**Alternatives considered:**
- `FloatingLabelInput`：技術正確但較長且使用者較不熟
- `TextField`（Material 官方術語）：與 HTML `<input>` 語意混淆

### D2：對外 `Value` / `ValueChanged`，內部 `@bind-value:event="oninput"`

外層維持標準 `@bind-Value` 慣例（呼叫端 `<MaterialInput @bind-Value="x.Name" />` 即可運作）。內部把 input event 從預設 `onchange` 改成 `oninput`，讓父層 model 在使用者打字時就更新，這是 Material 元件的典型互動期待。

**Alternatives considered:**
- 預設 `onchange` + 加 `OnInput` 開關：增加參數複雜度，違反 MVP
- 同時暴露 `OnInput` 事件：YAGNI，需要時再加

### D3：`Id` nullable + `OnInitialized` 內 fallback

```csharp
[Parameter] public string? Id { get; set; }
protected override void OnInitialized()
{
    Id ??= $"mi-{Guid.NewGuid():N}";
}
```

避免 field initializer `= Guid.NewGuid().ToString()` 在每次元件實例化時都換新值（造成 `@key` 切換時 label/input id 短暫不匹配）。`OnInitialized` 在元件生命週期內只跑一次，產生的 id 也只在此實例存活期間穩定。

### D4：不整合 EditContext

整合 `InputBase<T>` 會強制元件參與 Blazor 表單驗證生命週期、必須 override `TryParseValueFromString`、且限制只能放在 `<EditForm>` 內。對「我只想要一個漂亮的 input」場景太重。

**Alternatives considered:**
- 繼承 `InputBase<string>`：失去 EditForm 外的彈性
- 同時提供兩個元件（`MaterialInput` 與 `MaterialInputForForm`）：MVP 不需要

未來若有表單需求，新增一個獨立的 `MaterialInputField`（繼承 `InputText`）即可。

### D5：主題色用 arbitrary value `var(--color-primary)`

直接寫 `focus:border-[var(--color-primary)]` 與 `peer-focus:text-[var(--color-primary)]`。Tailwind CLI 會把字面值生成成 CSS class，與專案既有用法一致。

**Alternatives considered:**
- 在 Tailwind `@theme` 註冊 `--color-primary` 對應 token 後改用 `border-primary`：可行但需要動 `Styles/app.css` 並建立 token 命名規則，超出此 change 範圍

### D6：Sandbox 用獨立 `/playground` 頁

新增 `Pages/Playground.razor` + NavMenu 連結。理由：
- 不污染 Counter / Weather demo 既定用途
- 未來新增其他共享元件可累積在同一頁
- 路由 `/playground` 語意清楚

### D7：暫不寫 `MaterialInput.razor.css`（scoped CSS）

整支元件能用 Tailwind utility 表達，無 `::deep` 需求。維持 CLAUDE.md 中「優先用 Tailwind」的優先順序原則。

## Risks / Trade-offs

- **Tailwind JIT 偵測 peer-focus arbitrary value** → 第一次使用 `peer-focus:text-[var(--color-primary)]` 與 `focus:border-[var(--color-primary)]`，啟動 watch 後需檢查 `wwwroot/app.css` 確實生成；若沒生成則確認 watch session 與檔案存檔狀態
- **oninput 中文 IME 組字** → 「ㄢ」「ㄢˋ」中間狀態會即時推到 model；MVP 接受，未來若用在中文表單再加 composition 事件處理
- **stable id 仍是 GUID** → 對 a11y / 測試自動化而言不易預測；若呼叫端需要可預測 id（如 e2e test selector），請明確傳 `Id` 參數
- **Playground 頁公開可訪問** → 屬於開發用途，目前無 auth 機制；若未來上線需配合 build flag 或 auth 隱藏（不在本 change 範圍）

## Migration Plan

純新增、無 breaking change：
1. 新增 `Shared/` 資料夾與 `MaterialInput.razor`
2. 更新 `_Imports.razor`
3. 新增 `Pages/Playground.razor`
4. 更新 `NavMenu.razor`

無 rollback 顧慮（直接 `git revert` 即可，不影響其他頁面）。

## Open Questions

無；先前 explore session 已逐項拍板。
