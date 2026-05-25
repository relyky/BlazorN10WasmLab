## MODIFIED Requirements

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

#### Scenario: 群組包含 Playground 連結
- **WHEN** 「功能展示」群組展開
- **THEN** 子導覽項目 SHALL 包含「Playground」連結，href 為 `playground`，並使用與其他連結相同的 NavLink + SVG icon 格式

#### Scenario: Playground 頁面高亮
- **WHEN** 使用者位於 `/playground` 頁面
- **THEN** 側邊欄中 Playground 連結呈現 active 高亮樣式
