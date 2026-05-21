---
description: 分析 git 變更，建議 commit message 並執行提交
allowed-tools: >-
  Bash(git status:*)
  Bash(git diff:*)
  Bash(git log:*)
  Bash(git show:*)
  Bash(git branch:*)
  Bash(git tag:*)
  Bash(git remote:*)
  Bash(git ls-files:*)
  Bash(git blame:*)
  Bash(git rev-parse:*)
  Bash(git config --get:*)
  Bash(git stash list:*)
  Bash(git add:*)
  Bash(git rm:*)
  Bash(git mv:*)
  Bash(git restore --staged:*)
  Bash(git commit:*)
---

根據以下 git 變更資訊，建議一個 commit message，經確認後執行提交。

## Git Status

```
!`git status`
```

## Git Diff (staged)

```
!`git diff --staged`
```

## Git Diff (unstaged)

```
!`git diff`
```

## Recent Commits

```
!`git log --oneline -n 5`
```

## 規則

1. 使用繁體中文撰寫 commit message
2. 沿用專案風格前綴（首字大寫）：`Add:` 新功能、`Fix:` 修復、`Refactor:` 重構、`Update:` 更新、`Implement:` 實作整合
3. 第一行為摘要（重點描述「為何」而非「做了什麼」），不超過 72 字元
4. 視變更幅度補上「變更摘要」區塊，列出各檔案說明（格式：`- 檔案名稱 (變更說明)`）
5. 若變更涉及多個不相關修改，建議分開 commit 並列出各自的 message
6. 若沒有任何變更，回報「目前沒有待 commit 的變更」並結束

## 流程

1. 分析變更內容，產生建議的 commit message
2. 使用 AskUserQuestion 展示完整草擬訊息，提供選項：「同意並提交」、「修改訊息」、「取消」
3. 同意 → 執行 `git add . && git commit -m "..."`；修改 → 採用使用者輸入；取消 → 結束，不執行任何 git 操作
4. 提交完成後回報 commit hash 與檔案變更統計
5. 不執行 `git push`，除非使用者明確要求
