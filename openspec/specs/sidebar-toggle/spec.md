### Requirement: Logo 點擊切換側邊欄
AppBar 左側的 Logo 圓圈 SHALL 為可點擊按鈕，點擊時切換側邊欄的展開/收合狀態。

#### Scenario: 點擊 Logo 收合側邊欄
- **WHEN** 側邊欄為展開狀態，使用者點擊 AppBar Logo
- **THEN** 側邊欄收合（手機：滑出畫面；桌面：隱藏並釋放空間給 main）

#### Scenario: 點擊 Logo 展開側邊欄
- **WHEN** 側邊欄為收合狀態，使用者點擊 AppBar Logo
- **THEN** 側邊欄展開（手機：以 Overlay 滑入；桌面：以 inline 方式顯示）

### Requirement: 手機 Overlay 抽屜模式（< 768px）
在視窗寬度 < 768px 時，側邊欄 SHALL 以固定定位的 Overlay 抽屜呈現，不擠壓 main 內容。

#### Scenario: 手機展開時顯示遮罩
- **WHEN** 手機寬度下側邊欄展開
- **THEN** 側邊欄以 `fixed` 定位疊於內容上方，背景出現半透明黑色遮罩

#### Scenario: 點擊遮罩關閉側邊欄
- **WHEN** 手機寬度下側邊欄展開，使用者點擊遮罩區域
- **THEN** 側邊欄收合，遮罩消失

#### Scenario: 手機預設收合
- **WHEN** 頁面在手機寬度（< 768px）首次載入
- **THEN** 側邊欄預設為收合狀態

### Requirement: 桌面 inline 折疊模式（≥ 768px）
在視窗寬度 ≥ 768px 時，側邊欄 SHALL 以 inline 方式佔據 flex 空間，收合時完全隱藏並讓 main 佔滿剩餘寬度。

#### Scenario: 桌面預設展開
- **WHEN** 頁面在桌面寬度（≥ 768px）首次載入
- **THEN** 側邊欄預設為展開狀態

#### Scenario: 桌面收合後 main 佔滿
- **WHEN** 桌面寬度下側邊欄收合
- **THEN** main 內容區域自動擴展佔滿全寬，無遮罩

### Requirement: 展開/收合動畫
側邊欄展開與收合 SHALL 有平滑的滑入/出過渡動畫。

#### Scenario: 手機滑入動畫
- **WHEN** 手機寬度下點擊 Logo 展開側邊欄
- **THEN** 側邊欄從左側滑入，動畫時長約 200ms

#### Scenario: 手機滑出動畫
- **WHEN** 手機寬度下點擊 Logo 或遮罩收合側邊欄
- **THEN** 側邊欄向左滑出畫面，動畫時長約 200ms

### Requirement: 手機導覽後自動關閉、桌面保持展開
在手機 Overlay 模式下，點擊導覽項目後，側邊欄 SHALL 自動關閉。在桌面 inline 模式下，點擊導覽項目後，側邊欄 SHALL 保持展開狀態。

#### Scenario: 手機點擊 NavLink 後自動關閉
- **WHEN** 手機寬度（< 768px）下側邊欄展開，使用者點擊任一導覽連結（首頁、Counter、Weather）
- **THEN** 側邊欄自動收合，使用者看到目標頁面內容

#### Scenario: 桌面點擊 NavLink 後保持展開
- **WHEN** 桌面寬度（≥ 768px）下側邊欄展開，使用者點擊任一導覽連結
- **THEN** 側邊欄保持展開狀態，內容區切換至目標頁面
