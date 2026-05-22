### Requirement: 頂部品牌 Header 常駐顯示
應用程式 SHALL 在所有頁面頂部顯示一個固定高度的品牌 Header Bar，包含應用名稱與品牌識別色（primary color `#c2185b`）。

#### Scenario: Header 出現在所有頁面
- **WHEN** 使用者瀏覽任何路由（`/`、`/counter`、`/weather`）
- **THEN** 頂部 Header Bar 始終可見，不隨頁面內容滾動消失

#### Scenario: Header 顯示應用名稱
- **WHEN** 頁面載入完成
- **THEN** Header 左側顯示品牌 Logo 圓圈（字母 "B"）與應用名稱文字

### Requirement: 左側可折疊側邊欄導覽
應用程式 SHALL 提供左側固定寬度的側邊欄，包含可折疊的群組導覽項目。

#### Scenario: 側邊欄群組預設展開
- **WHEN** 頁面首次載入
- **THEN** 側邊欄的「功能展示」群組預設為展開狀態，所有子項目可見

#### Scenario: 點擊群組標頭折疊/展開
- **WHEN** 使用者點擊群組標頭（例如「功能展示」）
- **THEN** 該群組的子導覽項目收合或展開（toggle）

#### Scenario: 目前頁面高亮
- **WHEN** 使用者位於 `/counter` 頁面
- **THEN** 側邊欄中 Counter 連結呈現 active 高亮樣式

### Requirement: 品牌色系整合
應用程式 SHALL 使用 Tailwind v4 `@theme` 定義品牌色系，使 `bg-primary`、`text-primary` 等 utility class 全域可用。

#### Scenario: 品牌色應用於 Header
- **WHEN** 任何頁面渲染完成
- **THEN** 頂部 Header 背景色為 `--color-primary`（`#c2185b`），文字為白色

#### Scenario: 品牌色應用於側邊欄 active 狀態
- **WHEN** 目前頁面對應的導覽項目為 active
- **THEN** 該項目的 active 背景或文字顏色使用品牌色
