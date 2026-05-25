## Context

`BlazorN10WasmLab.Shared` 是 gRPC code-first 合約專案，內含 `[ServiceContract]` interface 與 `[ProtoContract]` DTO。當 Blazor Client 在 `BlazorN10WasmLab.Client/Shared/` 加入共享 UI 元件資料夾後，「Shared」一詞同時指涉兩個完全不同的層面：跨專案 wire 合約 vs. 前端 UI 共享元件。本次改名解決此命名衝突。

## Goals / Non-Goals

**Goals:**
- 專案 csproj、實體資料夾、assembly name、namespace 全數同步改為 `BlazorN10WasmLab.Contracts`
- 扁平化 `Contracts/` 子資料夾，避免 `BlazorN10WasmLab.Contracts.Contracts.*` 雙重命名
- 保持 git rename 偵測能追溯 blame 歷史
- 兩次 build 應完全可重現原本行為（純命名重構）

**Non-Goals:**
- 不變更 gRPC wire format、service / method 名稱（protobuf-net.Grpc 命名規則由 interface 名稱決定，`IWeatherService` 不改）
- 不變更套件相依
- 不修改已 archive 的歷史 OpenSpec change 文件
- 不擴充或重組合約內容

## Decisions

### 決策 1：namespace 同步改名（而非只改 csproj）
- **選擇**：csproj + 資料夾 + namespace 全改 `BlazorN10WasmLab.Contracts.*`
- **理由**：若僅改 csproj，`using BlazorN10WasmLab.Shared.Contracts;` 仍存在，原本痛點未解決
- **替代方案**：只改 csproj（駁回，治標不治本）

### 決策 2：扁平化 `Contracts/` 子資料夾
- **選擇**：`IWeatherService.cs`、`WeatherForecast.cs`、`WeatherForecastReply.cs` 上移至專案根，namespace = `BlazorN10WasmLab.Contracts`；`Surrogates/` 保留，namespace = `BlazorN10WasmLab.Contracts.Surrogates`
- **理由**：避免 `BlazorN10WasmLab.Contracts.Contracts.IWeatherService` 雙重 Contracts；Surrogates 是不同關注點（序列化轉接器），保留有語意價值
- **替代方案**：保留 `Contracts/` 子資料夾（駁回，視覺重複）

### 決策 3：搬移用普通檔案系統操作 + git auto-detect rename
- **選擇**：直接 move/edit，靠 `git status` rename 偵測（>50% 相似度）
- **理由**：namespace 字串改動小，相似度應仍能命中；省力
- **替代方案 A**：`git mv` 顯式 rename（駁回，git 仍以內容相似度判斷，差別不大）
- **替代方案 B**：分兩 commit（先改 namespace 再 mv）（駁回，user 選擇單一 commit 路線）

### 決策 4：assembly name 由 csproj 檔名隱含帶動
- **選擇**：不在 csproj 顯式設 `<AssemblyName>` / `<RootNamespace>`
- **理由**：現況就是預設值，改 csproj 檔名即同步帶動，零維護
- **替代方案**：顯式設定（駁回，多餘）

### 決策 5：archive 內容不動，當前 spec 必須更新
- **選擇**：`openspec/changes/archive/**` 完全不動；`openspec/specs/grpc-web-communication/spec.md` 透過本次 change 的 spec delta 更新
- **理由**：archive 是歷史快照，竄改會偽造歷史；當前 spec 必須反映現狀

## Risks / Trade-offs

- **[git rename 偵測失敗]** → 若 `Contracts/IWeatherService.cs` → `IWeatherService.cs` + namespace 改動讓相似度跌破閾值，blame 歷史中斷 → 緩解：實際 commit 後檢查 `git log --follow --oneline IWeatherService.cs`，若中斷可考慮 amend 改用 `git mv`
- **[漏改引用]** → 7 個檔案的 using、3 個 csproj 的 ProjectReference 任一漏改即編譯錯誤 → 緩解：改完跑 `dotnet build BlazorN10WasmLab.slnx` 全 solution 驗證；驗證後啟動 app 跑 Weather 頁面確認 gRPC 通訊正常
- **[檔案搬移結果與設計脫鉤]** → 扁平化過程中可能漏移檔案、誤刪 `Surrogates\`、或留下空的 `Contracts\` 子資料夾 → 緩解：驗證階段第一步以 `proposal.md` 的「改名後」目錄樹為基準，逐項比對實際結構（csproj 檔名、5 個 .cs 檔位置、`Surrogates\` 是否保留、`Contracts\` 是否已刪除），先做結構檢查再跑 build，避免被編譯錯誤掩蓋的結構性問題
- **[本機 IDE 鎖檔]** → Visual Studio / Rider 開著時改 csproj 路徑可能造成方案載入失敗 → 緩解：執行前提醒關閉 IDE
- **[CLAUDE.md 漏改]** → 文件描述與實際不符會誤導未來 AI agent → 緩解：搜尋 `BlazorN10WasmLab.Shared` 全文比對殘留
