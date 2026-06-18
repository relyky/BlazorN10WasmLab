# Issue tracker: OpenSpec workflow

本 repo 不使用傳統 issue tracker。議題、規格與開發進程文件
都透過 **OpenSpec** 工作流管理，記入 `openspec/` 目錄。

## 目錄結構

- `openspec/specs/<capability>/spec.md` — 各能力當前規格（requirements + scenarios）
- `openspec/changes/<name>/` — 進行中的變更，含 `proposal.md`、`design.md`、
  delta specs、`tasks.md`
- `openspec/changes/archive/YYYY-MM-DD-<name>/` — 已完成歸檔的變更

## 當 skill 說「publish to the issue tracker / 建立議題」

不要呼叫 `gh issue create`。改為發起一個 OpenSpec change：在
`openspec/changes/<name>/` 建立 proposal 與 tasks。優先使用斜線指令
`/opsx:propose`（提案）發起，`/opsx:apply`（實作）推進，
`/opsx:verify`（驗證）、`/opsx:archive`（歸檔同步 spec）收尾。

## 當 skill 說「fetch the relevant ticket / 讀取議題」

讀對應的 `openspec/changes/<name>/` 底下的 `proposal.md` 與 `tasks.md`；
若是已落地的能力，讀 `openspec/specs/<capability>/spec.md`。

## 修改既有能力

delta spec 走 `## MODIFIED Requirements`，完整貼上整個 requirement
區塊（保留所有 scenarios 並編輯）。
