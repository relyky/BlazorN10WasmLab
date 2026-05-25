## 1. 前置確認

- [x] 1.1 提醒使用者關閉 Visual Studio / Rider，避免方案載入鎖檔
- [x] 1.2 確認 working tree 乾淨或已 stash（避免改名與其他變更混淆）

## 2. 改名 Contracts 專案實體檔案

- [x] 2.1 將資料夾 `BlazorN10WasmLab.Shared\` 改名為 `BlazorN10WasmLab.Contracts\`
- [x] 2.2 將 csproj `BlazorN10WasmLab.Shared.csproj` 改名為 `BlazorN10WasmLab.Contracts.csproj`
- [x] 2.3 將 `Contracts\IWeatherService.cs`、`Contracts\WeatherForecast.cs`、`Contracts\WeatherForecastReply.cs` 上移至專案根
- [x] 2.4 刪除空的 `Contracts\` 子資料夾

## 3. 更新 Contracts 專案內部 namespace

- [x] 3.1 `IWeatherService.cs`：`BlazorN10WasmLab.Shared.Contracts` → `BlazorN10WasmLab.Contracts`
- [x] 3.2 `WeatherForecast.cs`：同上
- [x] 3.3 `WeatherForecastReply.cs`：同上
- [x] 3.4 `GrpcTypeModelSetup.cs`：`BlazorN10WasmLab.Shared` → `BlazorN10WasmLab.Contracts`（含內部對 Surrogates 的 using）
- [x] 3.5 `Surrogates\DateOnlySurrogate.cs`：`BlazorN10WasmLab.Shared.Surrogates` → `BlazorN10WasmLab.Contracts.Surrogates`

## 4. 更新方案與專案引用

- [x] 4.1 `BlazorN10WasmLab.slnx`：Project Path 由 `BlazorN10WasmLab.Shared/BlazorN10WasmLab.Shared.csproj` 改為 `BlazorN10WasmLab.Contracts/BlazorN10WasmLab.Contracts.csproj`
- [x] 4.2 `BlazorN10WasmLab\BlazorN10WasmLab\BlazorN10WasmLab.csproj`：`ProjectReference` 路徑同步更新
- [x] 4.3 `BlazorN10WasmLab\BlazorN10WasmLab.Client\BlazorN10WasmLab.Client.csproj`：`ProjectReference` 路徑同步更新

## 5. 更新引用方原始碼 using

- [x] 5.1 `BlazorN10WasmLab\BlazorN10WasmLab\Program.cs`：`using BlazorN10WasmLab.Shared.*` → `BlazorN10WasmLab.Contracts.*`
- [x] 5.2 `BlazorN10WasmLab\BlazorN10WasmLab\Services\WeatherService.cs`：同上
- [x] 5.3 `BlazorN10WasmLab\BlazorN10WasmLab.Client\Program.cs`：同上
- [x] 5.4 `BlazorN10WasmLab\BlazorN10WasmLab.Client\Services\WeatherServiceClient.cs`：同上（含 method descriptor 的 service name 字串 `BlazorN10WasmLab.Shared.Contracts.WeatherService` → `BlazorN10WasmLab.Contracts.WeatherService`）
- [x] 5.5 `BlazorN10WasmLab\BlazorN10WasmLab.Client\Pages\Weather.razor`：同上
- [x] 5.6 `BlazorN10WasmLab\BlazorN10WasmLab.Client\_Imports.razor`：若有相關 using 一併更新

## 6. 更新文件

- [x] 6.1 `CLAUDE.md`：所有 `BlazorN10WasmLab.Shared` 提及處改為 `BlazorN10WasmLab.Contracts`；命名規則範例（`IWeatherService in BlazorN10WasmLab.Shared.Contracts`）對應更新
- [x] 6.2 `openspec/specs/grpc-web-communication/spec.md`：Requirement 標題與內文中的 `BlazorN10WasmLab.Shared` 改為 `BlazorN10WasmLab.Contracts`
- [x] 6.3 確認 `openspec/changes/archive/**` 完全未動

## 7. 驗證

- [x] 7.1 **目錄結構比對**：實際 `BlazorN10WasmLab.Contracts\` 樹狀結構與 `proposal.md` 的「改名後」段落逐項比對（csproj 檔名、根層 5 個 .cs 檔、`Surrogates\` 子資料夾與其中的 `DateOnlySurrogate.cs`、無殘留的 `Contracts\` 空資料夾）
- [x] 7.2 `dotnet restore BlazorN10WasmLab.slnx` 成功
- [x] 7.3 `dotnet build BlazorN10WasmLab.slnx` 成功（Debug + Release）
- [x] 7.4 啟動 Aspire：`dotnet run --project BlazorN10WasmLab.AppHost`，開啟 Weather 頁面確認 Unary 與 Streaming 兩種刷新都成功取得資料
- [x] 7.5 全文搜尋 `BlazorN10WasmLab.Shared` 確認除 `openspec/changes/archive/**` 外無殘留
- [x] 7.6 `git status` 確認檔案以 `renamed:` 形式顯示（rename 偵測命中）；若中斷則考慮 amend 改用 `git mv`

## 8. 收尾

- [ ] 8.1 提交 commit
- [ ] 8.2 執行 `/opsx:verify` 驗證 change 完整性
- [x] 8.3 執行 `/opsx:archive` 歸檔本次 change
