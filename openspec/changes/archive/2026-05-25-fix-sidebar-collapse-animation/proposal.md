## Why

桌機 (≥ 768px) sidebar 收合時設計上應有 200ms 平滑動畫，但實際表現為瞬間消失。原因是 `MainLayout.razor` 在關閉態加了 `md:hidden`（`display: none`），瀏覽器對 display 切換不觸發 transition，導致 `transition-transform duration-200` 形同虛設。此外首次 hydrate 從 SSR 預設狀態切到 JS 偵測寬度結果時，也會帶著 transition 跑一次非預期動畫。

## What Changes

- 重寫 `MainLayout.razor` 的 `SidebarClass`：移除 `md:hidden`，改用 `translate-x-0` ↔ `-translate-x-full` 配合 `md:ml-0` ↔ `md:-ml-60` 達成「視覺滑出 + 佈局收回」同步動畫
- 將 `transition-transform` 改為 `transition-[transform,margin]`，精準涵蓋兩個會變動的屬性，避免子元素 hover 被牽連
- 新增 `_initialized` 旗標：首次 render 與 hydrate 設定初始狀態時不掛 transition class，避免初始跳變
- 桌機收合動畫由「不可見」升級為「200ms 滑出 + main 同步展開」

## Capabilities

### New Capabilities
（無）

### Modified Capabilities
- `sidebar-toggle`：擴充「展開/收合動畫」需求，補上桌面滑入/滑出 scenario；新增「首次載入不帶動畫」需求

## Impact

- `BlazorN10WasmLab.Client/Layout/MainLayout.razor`：`SidebarClass` 字串重寫、`_initialized` 旗標、`OnAfterRenderAsync` 流程調整
- 純 Tailwind utility 變動，不需額外 scoped CSS
- Tailwind JIT 需掃描到新的任意值 utility `transition-[transform,margin]` 與負 margin `md:-ml-60`，現有 `@source` 設定已涵蓋 `.razor` 檔
- 不影響其他元件、不改 NavMenu、不動 gRPC 通訊
