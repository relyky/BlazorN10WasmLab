## 1. 修改 App.razor

- [x] 1.1 在 `BlazorN10WasmLab/BlazorN10WasmLab/Components/App.razor` 檔案尾端新增 `@code` 區塊，宣告 `private static readonly InteractiveWebAssemblyRenderMode WasmNoPrerender = new(prerender: false);`，並於上方加一行繁中註解說明關閉 prerender 的原因（避免 server 解析只在 client DI 註冊的服務）
- [x] 1.2 將第 13 行 `<HeadOutlet @rendermode="InteractiveWebAssembly" />` 改為 `<HeadOutlet @rendermode="WasmNoPrerender" />`
- [x] 1.3 將第 17 行 `<Routes @rendermode="InteractiveWebAssembly" />` 改為 `<Routes @rendermode="WasmNoPrerender" />`

## 2. 啟動驗證

- [x] 2.1 執行 `/launch start`，確認應用以 Aspire 啟動成功且 Tailwind watch 正常
- [x] 2.2 用 Chrome MCP 巡訪 `/`（Home），確認頁面正常渲染、無 500、console 無例外
- [x] 2.3 巡訪 `/counter`，確認 Click me 按鈕仍能遞增（回歸測試）
- [x] 2.4 巡訪 `/playground`，確認 MaterialInput 雙向綁定仍正常（回歸測試）
- [x] 2.5 巡訪 `/weather`，確認頁面正常載入：首見 skeleton（淡 primary 脈動條 5 列），gRPC 回來後切換為真實資料表格
- [x] 2.6 在 Weather 頁點「刷新（Streaming）」按鈕，確認 skeleton 顯示直到首筆抵達後再轉為真資料（B 方案行為）
- [x] 2.7 執行 `/launch stop` 收工

## 3. 提交

- [x] 3.1 以單一 commit 提交 `App.razor` 變更，commit message 採用 `Fix:` 前綴並說明修復 Weather prerender DI 例外
