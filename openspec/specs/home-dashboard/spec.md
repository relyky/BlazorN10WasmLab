### Requirement: 靜態公告 Banner
首頁 SHALL 在頁面頂部顯示一個靜態公告 Banner，以卡片樣式呈現固定的公告文字。

#### Scenario: Banner 顯示於首頁頂部
- **WHEN** 使用者導覽至 `/`（首頁）
- **THEN** 頁面頂部出現公告 Banner，包含日曆圖示、主旨標題與說明文字

#### Scenario: Banner 不出現在其他頁面
- **WHEN** 使用者導覽至 `/counter` 或 `/weather`
- **THEN** 公告 Banner 不顯示（Banner 只屬於 Home.razor）

### Requirement: 功能入口卡片 Grid
首頁 SHALL 以網格排列方式顯示各功能頁面的入口卡片。

#### Scenario: 卡片顯示功能名稱與識別字母
- **WHEN** 首頁載入完成
- **THEN** 每張卡片顯示：彩色圓形（含首字母）、功能名稱（中文）、英文副標題

#### Scenario: 點擊卡片導覽至對應頁面
- **WHEN** 使用者點擊 "Counter 計數器" 卡片
- **THEN** 應用程式導覽至 `/counter`

#### Scenario: 點擊天氣卡片
- **WHEN** 使用者點擊 "Weather 天氣預報" 卡片
- **THEN** 應用程式導覽至 `/weather`

#### Scenario: Grid 自適應欄數
- **WHEN** 在較寬的螢幕（≥ 768px）上顯示
- **THEN** 卡片以至少 2 欄方式排列
