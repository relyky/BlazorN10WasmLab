## Context

`MainLayout.razor:55-78` 內有兩處重複的 viewport 判斷：`OnAfterRenderAsync(firstRender)` 用 `JS.InvokeAsync<int>("eval", "window.innerWidth")` + `>= 768` 設 sidebar 預設狀態；`CloseSidebarIfMobile()` 再 eval 一次配 `< 768`。共用 magic number `768`（要對齊 Tailwind `md:`）、共用 JS interop pattern，但「目前是不是 mobile」概念沒被模型化。

額外發現：完全沒監聽 resize 事件 → 拖動視窗跨越斷點時 sidebar 狀態不同步（latent UX bug）。

本變更同時處理「抽出 seam」與「修 latent bug」兩件事，並透過 OpenSpec capability 把這個 client-side 模組記入規格。

## Goals / Non-Goals

**Goals:**
- 把 viewport breakpoint 概念抽成可注入服務，消除 `MainLayout` 內重複的 JS interop 與 magic number
- 順手修掉 resize bug — 跨越斷點時 sidebar 自動同步
- 建立可重用 seam，未來其他元件需要 mobile detection 直接注入

**Non-Goals:**
- 不支援多斷點（`sm/lg/xl/2xl`）— 目前只需 `md:` 一條線
- 不引入 Reactive Extensions、INotifyPropertyChanged 等大型抽象
- 不改變 sidebar 的點擊行為（toggle button、nav click close-on-mobile 都保持）
- 不為其他 UI 元件重構（NavMenu、Header 等不動）

## Decisions

### D1：Push-based + 含 resize 訂閱（不是 pull snapshot）

**選項：**
- A. Pull only — `bool IsMobile()` 隨叫隨算，凍結現狀 bug
- B. Push + resize 訂閱 — 模組內封裝 listener，順手修 bug
- C. Hybrid lazy listener — 只在第一個 subscriber 出現時才裝 listener

**選擇：B**

**理由：**
- skill 原則「The interface is the test surface」— 既然要設計 seam，直接設成 reactive，避免未來再開第二輪「補 resize handler」重構
- A 只搬動程式碼、不消除 bug
- C 在「只有一個 consumer」場景毫無實質好處，徒增 lifecycle 心智負擔

### D2：`window.matchMedia` 而非 `window.addEventListener('resize')`

**選項：**
- A. matchMedia — 原生 breakpoint observer，事件只在「跨越斷點」觸發
- B. resize + width 比對 — 每次 resize 都觸發，需 C# / JS 端 throttle

**選擇：A**

**理由：**
- matchMedia 天然只在跨越斷點觸發，零後端 throttle 工作
- media query 字串 `(max-width: 767.98px)` 與 Tailwind `md:` 斷點語義一致，未來改斷點時這裡也跟著改
- B 等於重新發明 matchMedia

### D3：介面 surface — `IsMobile` + parameterless event + `EnsureInitializedAsync`

```csharp
public interface IViewportBreakpoint
{
    bool IsMobile { get; }
    event Action OnChanged;
    ValueTask EnsureInitializedAsync();
}
```

**為何 parameterless event 而非 `Action<bool>`：**
- 單一事實源（property），避免事件 dispatch 過程中 stale payload race
- 消費端模式統一：handler 內固定重讀 `IsMobile`

**為何顯式 `EnsureInitializedAsync`：**
- JS interop 必須在 client render 完成後才能呼叫；async factory + DI 註冊在 Blazor 沒原生支援
- `EnsureInitializedAsync` idempotent，呼叫端不需擔心順序
- `IsMobile` 在 init 前回傳 `false`（桌機預設），init 後為 matchMedia 即時值

### D4：實作 `IAsyncDisposable`（完整 JS listener cleanup）

**選項：**
- A. 完整 dispose（呼叫 JS 端 `removeEventListener`）
- B. 不 dispose（接受 tab 關閉時 OS 回收）

**選擇：A**

**理由：**
- 防止 dev hot reload 時 listener leak（重複觸發雖然冪等但浪費）
- 未來若改 lifetime 為 Scoped 時自動安全
- 多 15 行樣板，但搭配 JS module 對稱 init / dispose API 心智負擔小

### D5：Breakpoint 字串為 C# const，JS module 當 dumb adapter

**選項：**
- A. 寫死在 C# const，傳入 JS
- B. 寫死在 JS module
- C. C# 與 JS 各寫一份

**選擇：A**

**理由：**
- C# 是 source of truth；未來想擴充多斷點時 JS 不需動
- 字串：`(max-width: 767.98px)` — Tailwind `md:` 從 `>= 768px` 開始，所以 mobile 是反向；`767.98px` 避免邊界 fractional pixel 衝突

### D6：JS module 放在 Client 的 wwwroot（不放 Server）

`BlazorN10WasmLab.Client/wwwroot/js/viewport-breakpoint.js`

**理由：**
- JS module 是 client-side 行為，跟 `ViewportBreakpoint.cs` 應共住一個 project（locality）
- Blazor 框架自動把 client wwwroot 合併到 host serve path
- 未來若 client 單獨抽出帶得走

### D7：DI Singleton lifetime

- Blazor WASM 只有一個 user circuit，Singleton / Scoped 行為等效
- 慣例上 cross-cutting service（Auth / Toast / Localization）用 Singleton，本服務同類

### D8：跨斷點時 sidebar 行為 — 重置策略

跨到 mobile → `_sidebarOpen = false`；跨到 desktop → `_sidebarOpen = true`。**忘記跨斷點之間的使用者偏好**。

**理由：**
- 符合 responsive layout 的使用者直覺（mobile 不該預設展開、desktop 不該預設收起）
- 跟首次渲染預設行為一致（`>= 768` → 開、`< 768` → 關）
- 比保留「使用者最後 toggle 狀態」簡單，且該偏好在跨斷點後語義通常已失效

### D9：登錄 CONTEXT.md 詞彙

新增 repo 根目錄 `CONTEXT.md`，註冊 "Viewport breakpoint" 詞彙條目，對應 capability `viewport-breakpoint`。為未來累積 domain language 起頭。

## Risks / Trade-offs

- **行為變更**：使用者習慣的「resize 後 sidebar 不動」行為被改為「自動同步」 → 這是 UX 改進，但仍應 SIT 驗收
- **JS module + DotNetObjectReference 樣板**：第一次引入這個 pattern，後續若有第二個 JS module 服務可參考此範本
- **Init 前 `IsMobile = false` 預設值**：若使用者首次載入時是 mobile 寬度，在 WASM 啟動到 `EnsureInitializedAsync` 完成之間的極短時間內 `IsMobile` 為 false（桌機）；MainLayout 用 `_initialized` flag 避開 transition 動畫，所以視覺無影響 → 可接受
- **未來擴充多斷點**：目前介面只支援 mobile/desktop 二元判斷。若需要 `sm/lg/xl` 多斷點，需重新設計介面（例：`event Action<BreakpointName>` 或多個 property）。本變更明確不處理此情境
