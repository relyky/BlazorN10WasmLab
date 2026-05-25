## ADDED Requirements

### Requirement: Playground 沙盒頁面
應用程式 SHALL 提供 `/playground` 路由作為共享元件視覺驗證的 sandbox 頁面，與既有功能展示頁（Counter / Weather）職責區隔。

#### Scenario: 路由可訪問
- **WHEN** 使用者導覽至 `https://blazorn10wasmlab.dev.localhost:7009/playground`
- **THEN** 頁面 SHALL 正常載入，標題顯示 "Playground"

#### Scenario: 頁面標題樣式一致
- **WHEN** Playground 頁渲染完成
- **THEN** h1 標題 SHALL 使用與 Counter 頁一致的樣式（`text-4xl font-bold text-[var(--color-primary)] my-4`）

### Requirement: Playground 展示 MaterialInput
Playground 頁 SHALL 至少包含兩個 `MaterialInput` 元件 demo，並即時呈現繫結值，以驗證元件功能。

#### Scenario: 一般文字輸入 demo
- **WHEN** Playground 頁載入
- **THEN** SHALL 顯示一個 `Type="text"` 的 `MaterialInput`（label 例如「姓名」），下方 SHALL 即時顯示目前繫結值

#### Scenario: Email 型別輸入 demo
- **WHEN** Playground 頁載入
- **THEN** SHALL 顯示另一個 `Type="email"` 的 `MaterialInput`（label 例如「Email」），下方 SHALL 即時顯示目前繫結值

#### Scenario: 即時值變化反映
- **WHEN** 使用者在任一 `MaterialInput` 內輸入字元
- **THEN** 對應 demo 區塊下方的「目前值」文字 SHALL 在每次按鍵後即時更新
