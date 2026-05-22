## 1. MainLayout 狀態與 JS Interop

- [x] 1.1 在 `MainLayout.razor` 加入 `@inject IJSRuntime JS`，並宣告 `private bool _sidebarOpen = false;`
- [x] 1.2 加入 `OnAfterRenderAsync`：`firstRender` 時呼叫 `JS.InvokeAsync<int>("eval", "window.innerWidth")`，若 >= 768 則設 `_sidebarOpen = true` 並 `StateHasChanged()`
- [x] 1.3 加入 `ToggleSidebar()` 與 `CloseSidebar()` 方法

## 2. MainLayout Header — Logo 改為按鈕

- [x] 2.1 將 `MainLayout.razor` 中的 Logo `<div>` 改為 `<button>`，加入 `@onclick="ToggleSidebar"`、`type="button"`、`aria-label="Toggle navigation"`，保留原有視覺 class

## 3. MainLayout Sidebar — 響應式樣式

- [x] 3.1 在 `MainLayout.razor` 的 sidebar `<nav>` 前，加入遮罩 `<div>`：僅在 `_sidebarOpen` 時顯示，class 包含 `fixed inset-0 z-30 bg-black/50 md:hidden`，`@onclick="CloseSidebar"`
- [x] 3.2 將 sidebar `<nav>` 的 class 改為由 `SidebarClass` 屬性提供，在 `@code` 中以 C# property 回傳完整 Tailwind class string：
  - 開啟：`fixed top-16 bottom-0 left-0 z-40 w-60 bg-white border-r border-gray-200 overflow-y-auto transition-transform duration-200 translate-x-0 md:static md:top-auto md:bottom-auto md:z-auto md:shadow-none md:shrink-0`
  - 收合：`fixed top-16 bottom-0 left-0 z-40 w-60 bg-white border-r border-gray-200 overflow-y-auto transition-transform duration-200 -translate-x-full md:static md:top-auto md:bottom-auto md:z-auto md:shadow-none md:shrink-0 md:hidden`

## 4. NavMenu — OnNavigated EventCallback

- [x] 4.1 在 `NavMenu.razor` 加入 `[Parameter] public EventCallback OnNavigated { get; set; }`
- [x] 4.2 為每個 NavLink 加入 `@onclick` handler，點擊時呼叫 `await OnNavigated.InvokeAsync()`（首頁、Counter、Weather 三個連結）
- [x] 4.3 在 `MainLayout.razor` 的 `<NavMenu>` 標籤加入 `OnNavigated="CloseSidebar"`

## 5. 建置與驗證

- [x] 5.1 執行 `dotnet build BlazorN10WasmLab.slnx` 確認無編譯錯誤
- [x] 5.2 啟動應用程式，在桌面寬度確認：預設展開、點 Logo 可收合/展開
- [x] 5.3 縮小瀏覽器寬度至 < 768px，確認：預設收合、點 Logo 出現 Overlay + 遮罩、點遮罩關閉
- [x] 5.4 手機模式下點選 Counter/Weather NavLink，確認 sidebar 自動關閉
- [x] 5.5 確認滑入/出動畫約 200ms 正常運作
