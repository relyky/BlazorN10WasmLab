## MODIFIED Requirements

### Requirement: 展開/收合動畫
側邊欄展開與收合 SHALL 有平滑的滑入/出過渡動畫，桌機與手機皆然。桌機收合時 sidebar SHALL 同步釋放佈局空間，視覺與佈局動畫同步進行。

#### Scenario: 手機滑入動畫
- **WHEN** 手機寬度下點擊 Logo 展開側邊欄
- **THEN** 側邊欄從左側滑入，動畫時長約 200ms

#### Scenario: 手機滑出動畫
- **WHEN** 手機寬度下點擊 Logo 或遮罩收合側邊欄
- **THEN** 側邊欄向左滑出畫面，動畫時長約 200ms

#### Scenario: 桌機滑出動畫
- **WHEN** 桌機寬度（≥ 768px）下點擊 Logo 收合側邊欄
- **THEN** 側邊欄向左滑出視覺，同時佔據的佈局寬度同步收回，main 區域隨之擴展，動畫時長約 200ms

#### Scenario: 桌機滑入動畫
- **WHEN** 桌機寬度下點擊 Logo 展開側邊欄
- **THEN** 側邊欄從左側滑入佈局，同時 main 區域同步壓縮讓出空間，動畫時長約 200ms

## ADDED Requirements

### Requirement: 首次載入不觸發動畫
首次頁面載入並完成 hydrate 偵測視窗寬度設定初始 sidebar 狀態時，SHALL 不觸發展開/收合過渡動畫，避免使用者看到非預期的初始動畫。

#### Scenario: 桌機首次載入直接顯示展開態
- **WHEN** 桌機寬度首次載入頁面，hydrate 完成將 sidebar 設為展開
- **THEN** sidebar 直接呈現展開狀態，無 200ms 滑入動畫

#### Scenario: 手機首次載入直接顯示收合態
- **WHEN** 手機寬度首次載入頁面，hydrate 完成確認 sidebar 為收合
- **THEN** sidebar 直接呈現收合狀態（畫面外），無 200ms 滑出動畫

#### Scenario: hydrate 完成後點擊才觸發動畫
- **WHEN** 首次 hydrate 完成後使用者點擊 Logo
- **THEN** sidebar 切換狀態時帶有 200ms 過渡動畫
