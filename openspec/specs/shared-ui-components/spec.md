### Requirement: 共享元件資料夾與命名空間
Blazor Client SHALL 提供 `BlazorN10WasmLab.Client/Shared/` 資料夾作為可重用 UI 元件落點，並使該資料夾的 namespace `BlazorN10WasmLab.Client.Shared` 透過 `_Imports.razor` 全域可用。

#### Scenario: 資料夾結構存在
- **WHEN** 開發者瀏覽 `BlazorN10WasmLab.Client` 專案結構
- **THEN** 看到 `Shared/` 資料夾位於專案根層，與 `Pages/`、`Layout/`、`Services/` 平行

#### Scenario: 不需明寫 namespace
- **WHEN** 任何 `.razor` 檔案使用共享元件（例如 `<MaterialInput />`）
- **THEN** 不需額外撰寫 `@using BlazorN10WasmLab.Client.Shared`，元件可直接使用

### Requirement: MaterialInput 浮動標籤輸入元件
共享元件 `MaterialInput` SHALL 提供 Material 風格的浮動標籤文字輸入框，支援 `string?` 雙向繫結，採用專案主題色 `var(--color-primary)`。

#### Scenario: 元件可被父層繫結
- **WHEN** 父元件以 `<MaterialInput @bind-Value="model.Name" />` 使用
- **THEN** input 內容變更時，`model.Name` 同步更新

#### Scenario: 輸入時即時更新（oninput）
- **WHEN** 使用者在 input 內逐字輸入
- **THEN** `ValueChanged` 事件 SHALL 在每次按鍵後即時觸發（不等失焦）

#### Scenario: 預設空值顯示 label 在 input 內位置
- **WHEN** 元件初始載入且 `Value` 為 null 或空字串
- **THEN** label 文字 SHALL 顯示在 input 上方輸入區的初始位置（未浮動），底線色為 `slate-200`

#### Scenario: focus 時 label 浮動且顏色為主題色
- **WHEN** 使用者點擊 input 取得焦點
- **THEN** label 文字 SHALL 上浮至 input 上方並縮小，顏色變為 `var(--color-primary)`，input 底線色亦變為 `var(--color-primary)`

#### Scenario: 有值時失焦後 label 維持浮動
- **WHEN** input 有非空白值且失去焦點
- **THEN** label 文字 SHALL 維持在浮動位置（不回到原位），但顏色與底線恢復為 neutral slate

#### Scenario: 無值時失焦後 label 回原位
- **WHEN** input 為空且失去焦點
- **THEN** label 文字 SHALL 回到 input 內的原始位置

### Requirement: MaterialInput 元件參數
`MaterialInput` SHALL 接受以下參數，其中 `Id` 為選填且能自動生成穩定值。

#### Scenario: 必要參數
- **WHEN** 父元件使用 `<MaterialInput />`
- **THEN** 元件 SHALL 接受 `Value` (string?)、`ValueChanged` (EventCallback<string?>)、`Label` (string，預設 "欄位名稱")、`Type` (string，預設 "text")、`Class` (string，預設 "max-w-sm")

#### Scenario: Id 自動生成
- **WHEN** 父元件未傳遞 `Id` 參數
- **THEN** 元件 SHALL 在 `OnInitialized` 階段生成 `mi-{32位 hex}` 格式的 stable id，且該 id 在元件存活期間保持不變

#### Scenario: Id 可由父層指定
- **WHEN** 父元件傳遞 `Id="my-input"` 參數
- **THEN** input 元素的 `id` 屬性 SHALL 為 `"my-input"`，label 的 `for` 屬性 SHALL 對應相同值

#### Scenario: Type 透傳至 input
- **WHEN** 父元件傳遞 `Type="email"`
- **THEN** 渲染後的 `<input>` 元素 `type` 屬性 SHALL 為 `"email"`

### Requirement: MaterialInput 不整合 EditContext
`MaterialInput` SHALL NOT 整合 Blazor 的 `EditContext` / `InputBase<T>` 驗證機制，且元件原始碼 SHALL 於頂部以註解清楚註明此限制。

#### Scenario: EditForm 內請改用 InputText
- **WHEN** 開發者閱讀 `MaterialInput.razor` 原始碼
- **THEN** 檔案頂部 SHALL 存在註解說明「此元件未整合 EditContext，在 `<EditForm>` 內請改用 `<InputText>`」
