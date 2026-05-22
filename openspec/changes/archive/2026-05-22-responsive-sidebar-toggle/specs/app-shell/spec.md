## MODIFIED Requirements

### Requirement: 頂部品牌 Header 常駐顯示
應用程式 SHALL 在所有頁面頂部顯示一個固定高度的品牌 Header Bar，包含應用名稱與品牌識別色（primary color `#c2185b`）。Header 左側的 Logo 圓圈 SHALL 為可點擊按鈕，用於切換側邊欄展開/收合狀態。

#### Scenario: Header 出現在所有頁面
- **WHEN** 使用者瀏覽任何路由（`/`、`/counter`、`/weather`）
- **THEN** 頂部 Header Bar 始終可見，不隨頁面內容滾動消失

#### Scenario: Header 顯示應用名稱
- **WHEN** 頁面載入完成
- **THEN** Header 左側顯示品牌 Logo 圓圈（字母 "B"）與應用名稱文字

#### Scenario: Logo 為可點擊按鈕
- **WHEN** 使用者點擊 Header 左側的 Logo 圓圈
- **THEN** 側邊欄切換展開/收合狀態（toggle）
