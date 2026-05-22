## Context

`MainLayout.razor` 目前有固定 `w-60` 的側邊欄，無法收合。本次需要加入響應式 toggle：手機用 Overlay 抽屜，桌面用 inline 折疊，兩者均透過點擊 AppBar Logo 控制。

現有架構：`MainLayout.razor`（header + nav + main），`NavMenu.razor`（群組導覽），兩者無父子通訊機制。

## Goals / Non-Goals

**Goals:**
- Logo 點擊 → 切換 `_sidebarOpen` 狀態
- xs（< 768px）：sidebar 以 `fixed` Overlay 呈現，搭配半透明遮罩
- md+（≥ 768px）：sidebar 以 `static` inline 呈現，收合時 `hidden`
- 初次載入以 JS interop 偵測 `window.innerWidth`，決定初始狀態
- 手機點選 NavLink 後自動關閉 sidebar
- CSS `transition-transform duration-200` 動畫

**Non-Goals:**
- Mini sidebar（icon-only 模式）
- 桌面側邊欄寬度可拖拉調整
- 使用者偏好持久化（localStorage）
- 響應式字體/間距調整

## Decisions

### 1. 狀態管理：單一 `_sidebarOpen` bool 在 MainLayout

**選擇**：`MainLayout.razor` 持有 `private bool _sidebarOpen`，所有 sidebar 行為由此驅動。

**理由**：sidebar 的展開/收合影響整個 layout（遮罩、main 寬度），狀態自然歸屬 MainLayout。NavMenu 只需透過 EventCallback 通知「有導覽發生」，不需知道 sidebar 狀態。

### 2. 初始狀態偵測：JS interop（inline eval）

**選擇**：`OnAfterRenderAsync(firstRender: true)` 中呼叫：
```csharp
var width = await JS.InvokeAsync<int>("eval", "window.innerWidth");
_sidebarOpen = width >= 768;
StateHasChanged();
```

**理由**：Blazor WASM 在 client 端渲染，server-side 無法知道螢幕寬度。`eval` 方式無需額外 .js 檔案，最小侵入性。代價是初次渲染會有一個 cycle 的閃爍（從 `false` 切換到 `true`），在正常網路速度下幾乎不可察覺。

**替代放棄**：`window.matchMedia` listener — 可監聽 resize，但複雜度過高，超出 MVP 範圍。

### 3. xs Overlay vs md+ inline：純 CSS 斷點切換

**選擇**：sidebar `<nav>` 的 Tailwind class 組合同時描述兩種 layout：
- 開啟：`fixed top-16 inset-y-0 left-0 z-40 w-60 translate-x-0 md:static md:z-auto md:top-auto`
- 收合：`fixed top-16 inset-y-0 left-0 z-40 w-60 -translate-x-full md:static md:z-auto md:top-auto md:hidden`

`md:static` 讓桌面版回到正常 flow，`-translate-x-full` 讓手機版滑出畫面，`md:hidden` 讓桌面版完全消失。

**理由**：一份 HTML，CSS 驅動兩種視覺行為，不需兩套 DOM。

### 4. NavMenu 通知：EventCallback（不用 CascadingParameter）

**選擇**：`NavMenu.razor` 加入 `[Parameter] public EventCallback OnNavigated { get; set; }`，每個 NavLink 點擊時 invoke。

**理由**：EventCallback 是 Blazor 標準父子通訊模式，比 CascadingParameter 或靜態 service 更輕量、更明確，且不引入全域狀態。

## Risks / Trade-offs

- **初始渲染閃爍（xs）** → 首次載入 `_sidebarOpen = false`，JS 判斷後若 md+ 則設為 true；xs 使用者不受影響，md+ 使用者可能看到 sidebar 瞬間展開（< 100ms）→ 可接受
- **`eval` 安全性** → 僅用於讀取 `window.innerWidth`，無使用者輸入，風險極低
- **Tailwind class purge** → 動態組合的 class 字串需確保 Tailwind v4 scanner 能掃到；使用 `@source` 已涵蓋所有 `.razor` 檔，完整 class 字串寫在 `@code` 的 property 中亦可被掃描
