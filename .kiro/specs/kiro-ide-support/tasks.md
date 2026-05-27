# Implementation Plan: Kiro IDE Support

## Overview

将 Kiro IDE 作为新的受管客户端接入 CodeIsland。整体路径严格按照 AGENTS.md 「If you add a Claude-compatible hook client, start in `CodeIsland/Models/ClientProfile.swift`」的指引：先扩展 `SessionClientBrand` 枚举与 logo 资源，再向 `ClientProfileRegistry` 的 `managedHookProfiles` / `runtimeProfiles` / `ideExtensionProfiles` 三张静态数组各追加一条 Kiro 条目，最后把 `MascotClient` / `MascotKind` 与渲染方法对齐。每一步都尽量沿用既有的 Cursor / CodeBuddy / Qoder 注册模式，并在每个实现 sub-task 后追加可选的单元测试覆盖。

## Tasks

- [x] 1. 准备 Kiro 品牌枚举与图标资源
  - [x] 1.1 在 SessionClientBrand 枚举中追加 .kiro case
    - 修改 `CodeIsland/Models/ClientProfile.swift` 顶部的 `SessionClientBrand` 枚举，追加 `case kiro`，rawValue 为 `"kiro"`
    - 该 case 用于后续 `ManagedHookClientProfile` / `SessionClientProfile` 的 `brand` 字段以及 `MascotClient.init` 的 brand 兜底分支
    - _Requirements: 1.9, 7.3_

  - [x] 1.2 添加 KiroLogo imageset 资源
    - 在 `CodeIsland/Assets.xcassets/KiroLogo.imageset/` 下创建 `Contents.json` 与占位 `kiro-logo.png`
    - `Contents.json` 沿用 `ClaudeLogo.imageset` 的 universal idiom + 1x 结构，仅指向 `kiro-logo.png`
    - PNG 占位文件由设计师后续替换；保证构建期资源可被 `Image("KiroLogo")` 解析
    - _Requirements: 5.1_

- [x] 2. 注册 Kiro 受管 Hook 客户端 Profile
  - [x] 2.1 在 ClientProfileRegistry.managedHookProfiles 中追加 kiro-hooks 条目
    - 修改 `CodeIsland/Models/ClientProfile.swift` 的 `managedHookProfiles` 静态数组，追加 `ManagedHookClientProfile(id: "kiro-hooks", ...)`
    - `configurationRelativePath` 设为 `".kiro/settings.json"`，落入既有 `.jsonHooks` 安装分支
    - `bridgeSource` 设为 `"claude"`；`bridgeExtraArguments` 含 `--client-kind kiro`、`--client-name Kiro`、`--client-originator Kiro`
    - `localAppBundleIdentifiers` 含 `"com.amazon.kiro"`，`brand` 设为 `.kiro`，`defaultEnabled` 为 `false`
    - `logoAssetName` 设为 `"KiroLogo"`，`prefersBundledLogoOverAppIcon` 设为 `true`
    - `events` 列表覆盖 `PreToolUse`/`PostToolUse`/`Notification`/`PermissionRequest`(timeout 86400)/`Stop`/`SubagentStop`/`SessionStart`/`SessionEnd`/`UserPromptSubmit`/`PreCompact`(auto+manual)，全部使用 Claude 风格 matcher
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 2.10, 5.2, 5.3_

  - [ ] 2.2 编写 kiro-hooks profile 单元测试
    - 新建 `CodeIslandTests/KiroIntegrationTests.swift`，参照 `GeminiIntegrationTests` / `OpenCodeIntegrationTests` 风格
    - 添加 `testKiroManagedHookProfileMatchesContract()` 用例
    - 断言 `configurationRelativePaths == [".kiro/settings.json"]`、`installationKind == .jsonHooks`、`bridgeSource == "claude"`
    - 断言 `bridgeExtraArguments` 含 client-kind / name / originator，`defaultEnabled == false`，`brand == .kiro`，`localAppBundleIdentifiers` 含 `com.amazon.kiro`
    - 断言 `events` 集合的 name 与 templates 与设计文档完全一致
    - _Requirements: 1.1, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 2.10_

- [x] 3. 注册 Kiro 运行时 Profile
  - [x] 3.1 在 ClientProfileRegistry.runtimeProfiles 中追加 kiro 条目
    - 修改 `CodeIsland/Models/ClientProfile.swift` 的 `runtimeProfiles` 静态数组，追加 `SessionClientProfile(id: "kiro", ...)`
    - 设置 `provider: .claude`、`family: .claudeHooks`、`kind: .claudeCode`、`brand: .kiro`、`assistantLabelMode: .badgeLabel`
    - `displayName` 设为 `"Kiro"`，使会话徽标渲染 "Kiro"
    - `recognizedKinds` 设为 `["kiro", "kiro-ide", "kiro_ide", "kiro ide"]`，覆盖归一化后的 client-kind 命中
    - `exactAliases` 至少包含 `"kiro"`、`"kiro-ide"`、`"kiro ide"`，`keywordAliases` 含 `"kiro"`
    - `bundleIdentifiers` 含 `"com.amazon.kiro"`
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6_

  - [ ] 3.2 编写 runtime profile 单元测试
    - 在 `CodeIslandTests/KiroIntegrationTests.swift` 追加 `testKiroRuntimeProfileResolvesBrandAndMascot()`
    - 用 `ClientProfileRegistry.matchRuntimeProfile(provider: .claude, explicitKind: "kiro", ...)` 等场景断言返回 profile id `"kiro"` 且 `matchScore >= 100`
    - 对 `"kiro-ide"`、`"KIRO IDE"`（大小写归一化后）也命中并断言相同 profile
    - 仅给定 `originator: "Kiro"`、`explicitKind: nil` 时断言 `matchScore >= 60`
    - 构造 `SessionClientInfo(kind: .claudeCode, profileID: "kiro", ...)` 后断言 `brand == .kiro`、`badgeLabel(for: .claude) == "Kiro"`、`MascotClient(clientInfo:provider:) == .kiro`、`MascotKind(clientInfo:provider:) == .kiro`
    - _Requirements: 3.2, 3.3, 3.4, 3.5, 3.6, 7.1, 7.2_

- [x] 4. 注册 Kiro IDE 扩展 Profile 与窗口路由
  - [x] 4.1 在 ClientProfileRegistry.ideExtensionProfiles 中追加 kiro-extension 条目
    - 修改 `CodeIsland/Models/ClientProfile.swift` 的 `ideExtensionProfiles` 静态数组，追加 `ManagedIDEExtensionProfile(id: "kiro-extension", ...)`
    - `uriScheme` 设为 `"kiro"`，`extensionRootRelativePath` 设为 `".kiro-ide/extensions"`，`extensionRegistryRelativePath` 设为 `".kiro-ide/extensions/extensions.json"`
    - `logoAssetName` 设为 `"KiroLogo"`，`prefersBundledLogoOverAppIcon` 为 `true`
    - `exactBundleIdentifiers` 与 `localAppBundleIdentifiers` 含 `"com.amazon.kiro"`，`appNameKeywords` 含 `"kiro"`，`bundleIdentifierKeywords` 含 `"kiro"`
    - 不设置 `sessionFocusStrategy`，沿用默认 `/focus` 路径
    - _Requirements: 6.1, 6.2, 6.3, 6.5, 6.6, 6.7_

  - [x] 4.2 将 "kiro" 加入 prefersWorkspaceWindowRouting 的 switch 命中列表
    - 修改 `CodeIsland/Models/ClientProfile.swift` 中 `ManagedIDEExtensionProfile.prefersWorkspaceWindowRouting` 的 switch 分支
    - 在原本的 `"vscode"`, `"cursor"`, `"trae"`, `"codebuddy"`, `"qoder"`, `"qoder-work"` 之后追加 `"kiro"`，使 `SessionLauncher.routeIDEWorkspaceWindow` 能复用 VS Code 风格工作区路由
    - _Requirements: 6.4_

  - [ ] 4.3 编写 IDE 扩展 profile 与 URI 路由单元测试
    - 在 `CodeIslandTests/KiroIntegrationTests.swift` 追加 `testKiroIDEExtensionProfileMatchesBundleAndAppName()` 与 `testKiroExtensionURIPreservesQueryParameters()`
    - 断言 `ClientProfileRegistry.ideExtensionProfile(bundleIdentifier: "com.amazon.kiro", appName: "Kiro")?.id == "kiro-extension"`
    - 断言 `ideExtensionProfile(bundleIdentifier: nil, appName: "Kiro")?.id == "kiro-extension"`
    - 断言 `uriScheme == "kiro"`、`extensionRootRelativePaths == [".kiro-ide/extensions"]`、`prefersWorkspaceWindowRouting == true`
    - 通过 `IDEExtensionInstaller.makeURI(profile:path:queryItems:)` 构造 `/focus` URI，断言 scheme 为 `kiro`、host 为 `code-island.session-focus`、`pid`/`sessionId`/`cwd` 透传
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.7_

  - [ ] 4.4 更新 ClientProfileIconTests 期望映射
    - 修改 `CodeIslandTests/ClientProfileIconTests.swift`
    - 在 `testManagedHookProfilesUseBundledLogosForSettings` 的期望 map 中追加 `"kiro-hooks": "KiroLogo"`
    - 在 `testIDEExtensionProfilesUseBundledLogosForSettings` 的期望 map 中追加 `"kiro-extension": "KiroLogo"`
    - _Requirements: 5.1, 5.2, 5.4, 5.5_

- [-] 5. Checkpoint - 验证 hook / runtime / IDE 配置层
  - 运行 `xcodebuild -project CodeIsland.xcodeproj -scheme CodeIsland -configuration Debug CODE_SIGNING_ALLOWED=NO test -only-testing:CodeIslandTests` 与 `swift test --package-path Prototype`
  - Ensure all tests pass, ask the user if questions arise.

- [x] 6. 宠物系统集成
  - [x] 6.1 在 MascotView 中实现 MascotKind.kiro 与 drawKiro 渲染
    - 修改 `CodeIsland/UI/Components/MascotView.swift` 的 `MascotKind` 枚举，追加 `case kiro`
    - 设置 `title = "Kiro"`，中文 `subtitle`（2–6 字宠物名称，如「紫鳞水母」）
    - 定义 `alertColor = Color(red: 0.62, green: 0.40, blue: 0.94)`；为 11 项既有 alertColor 与 `.kiro` 的 RGB 通道做最小差校验，确保至少一通道差 ≥ 0.05
    - 实现 `private func drawKiro(in context: GraphicsContext, canvasSize: CGSize, time: TimeInterval, mode: MascotRenderMode)`，沿袭仓库现有像素艺术风格，区分 idle / working / warning 状态
    - 在 `drawMascot` 主 switch 中追加 `case .kiro: drawKiro(...)`
    - _Requirements: 4.2, 4.5, 4.6, 4.7_

  - [x] 6.2 在 MascotView 中实现 MascotClient.kiro 与路由
    - 修改 `CodeIsland/UI/Components/MascotView.swift` 的 `MascotClient` 枚举，追加 `case kiro`
    - 在 `MascotClient.allCases` 静态数组末尾追加 `.kiro`
    - 设置 `title = "Kiro"`，中文 `subtitle` 描述 Kiro IDE 钩子源（沿用 Cursor / CodeBuddy 文案模式）
    - 设置 `defaultMascotKind = .kiro`
    - 在 `MascotClient.init(clientInfo:provider:)` 的 profileID switch 内追加 `case "kiro": .kiro`
    - 在 brand 兜底分支追加 `case .kiro: .kiro`
    - _Requirements: 4.1, 4.3, 4.4, 4.8, 7.1, 7.2, 7.4, 7.5_

  - [ ] 6.3 编写宠物系统集成单元测试
    - 在 `CodeIslandTests/KiroIntegrationTests.swift` 追加 `testKiroMascotEnumIntegrity()`
    - 断言 `MascotClient.allCases.contains(.kiro)`、`MascotClient.kiro.title == "Kiro"`、`MascotClient.kiro.defaultMascotKind == .kiro`
    - 断言 `MascotKind.allCases.contains(.kiro)`，并通过遍历断言 `.kiro.alertColor` 与每个其它 MascotKind alertColor 至少在一个 RGB 通道相差 ≥ 0.05
    - 构造 profileID 为 `"kiro"` 与 brand 为 `.kiro` 的 `SessionClientInfo`，分别断言 `MascotClient(clientInfo:provider:)` 命中 `.kiro`，验证 profileID 优先于 brand 兜底
    - _Requirements: 4.1, 4.2, 4.4, 4.5, 7.1, 7.2, 7.4, 7.5_

- [~] 7. 最终 Checkpoint - 跑全量测试
  - 运行 `./scripts/test.sh`，确保 `CodeIslandTests` / `Prototype` 全部通过
  - 通过 `xcodebuild ... build` 确认主 Xcode scheme 仍可编译
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- 标 `*` 的 sub-task 可选，可在追求 MVP 进度时跳过；但 `2.2*` / `3.2*` / `4.3*` / `4.4*` / `6.3*` 一同构成 Kiro 集成的回归基线，建议至少在合入前补齐
- Kiro 接入主要是 `ClientProfileRegistry` 三张静态数组 + 枚举追加，没有引入新分支文件或新数据流，故无需 PBT；本计划全程使用 example-based 单元测试覆盖配置正确性
- `MascotKind.alertColor` 的 RGB 通道差需要在测试里数值校验，避免后续误改其它 alertColor 时与 Kiro 撞色
- `drawKiro` 的视觉效果属于像素艺术，自动化测试无法覆盖，需在 `MascotSettingsView` Preview 中人工验证 idle / working / warning 三态
- `kiro-logo.png` 是占位资源，需要由设计师后续替换；imageset 结构本身在 1.2 完成

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "1.2"] },
    { "id": 1, "tasks": ["2.1", "6.1"] },
    { "id": 2, "tasks": ["3.1", "6.2", "2.2"] },
    { "id": 3, "tasks": ["4.1", "3.2"] },
    { "id": 4, "tasks": ["4.2", "4.4"] },
    { "id": 5, "tasks": ["4.3"] },
    { "id": 6, "tasks": ["6.3"] }
  ]
}
```
