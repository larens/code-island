# Git Guide / Git 协作规范

分支策略、提交规范和协作流程。

---

## English

This document defines the Git workflow for Code Island: branch naming conventions, commit message format, PR process, and merge strategy. All contributors should follow these norms to keep the history clean and reviewable.

## 中文

本文档定义了 Code Island 的 Git 工作流：分支命名约定、提交信息格式、PR 流程和合并策略。所有贡献者应遵循这些规范，以保持历史清晰可审查。

---

## Branch Naming / 分支命名

```
feature/xxx     # New feature / 新功能
fix/xxx         # Bug fix / 缺陷修复
docs/xxx        # Documentation / 文档
refactor/xxx    # Refactoring / 重构
```

## Commit Message / 提交信息

```
<type>(<scope>): <subject>

<body>
```

Types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`

## PR Process / PR 流程

1. Create a feature branch from `master`
2. Make changes with clear, focused commits
3. Open a PR with a descriptive title and summary
4. Ensure CI passes (`pr-checks.yml`)
5. Get at least one review before merging
6. Squash merge into `master`
