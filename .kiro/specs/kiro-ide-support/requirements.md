# Requirements Document

## Introduction

为 CodeIsland 新增 Kiro IDE 客户端支持。Kiro 是 AWS 推出的 AI 驱动 IDE，基于 VS Code 架构，支持 hooks 机制。本功能将 Kiro 作为新的受管客户端接入 CodeIsland，实现完成通知、选择/审批提示、进度监控，以及在设置中展示对应 logo 宠物状态。

## Glossary

- **Kiro**：AWS 推出的 AI 驱动 IDE 编辑器，基于 VS Code 架构，内置 hooks 机制
- **CodeIsland**：macOS 菜单栏应用，以 Dynamic Island 风格展示 AI 编码代理的状态
- **ManagedHookClientProfile**：受管 hook 客户端配置，定义 hook 安装路径、事件列表和 bridge 参数
- **SessionClientProfile**：运行时客户端配置，用于会话识别和品牌匹配
- **MascotClient**：宠物系统中的客户端枚举，每个客户端对应一个默认宠物形象
- **MascotKind**：宠物形象枚举，定义可用的宠物动画种类
- **HookInstaller**：负责安装和管理各客户端 hook 配置的服务
- **Bridge**：CodeIslandBridge 二进制，作为 hook 事件的统一入口，负责上下文捕获和事件转发
- **ManagedIDEExtensionProfile**：受管 IDE 扩展配置，用于终端聚焦跳转功能

## Requirements

### Requirement 1: Kiro Hook 客户端配置注册

**User Story:** 作为 CodeIsland 用户，我希望 Kiro 出现在受管 hook 客户端列表中，以便我可以在设置中启用/禁用 Kiro 的 hook 集成。

#### Acceptance Criteria

1. THE ClientProfileRegistry SHALL include a ManagedHookClientProfile entry with id "kiro-hooks" in the managedHookProfiles array
2. IF Kiro is not installed on the system (as determined by its bundle identifier not being found by ClientAppLocator) AND alwaysVisibleInSettings is false, THEN THE HookInstaller SHALL exclude the Kiro profile from the manageable client list presented in settings
3. THE ManagedHookClientProfile for Kiro SHALL specify configurationRelativePath as ".kiro/settings.json" relative to the user home directory, with installationKind set to .jsonHooks
4. THE ManagedHookClientProfile for Kiro SHALL use "claude" as the bridgeSource to leverage the existing Claude-compatible hook protocol
5. THE ManagedHookClientProfile for Kiro SHALL include bridgeExtraArguments that set "--client-kind" to "kiro", "--client-name" to "Kiro", and "--client-originator" to "Kiro"
6. THE ManagedHookClientProfile for Kiro SHALL have defaultEnabled set to false, requiring explicit user opt-in
7. THE ManagedHookClientProfile for Kiro SHALL specify at least one localAppBundleIdentifiers entry containing Kiro's macOS bundle identifier so that ClientAppLocator can detect whether Kiro is installed
8. THE ManagedHookClientProfile for Kiro SHALL define a non-empty events array listing the hook event descriptors that CodeIslandBridge will register in .kiro/settings.json upon installation
9. THE SessionClientBrand enum SHALL include a case that the Kiro ManagedHookClientProfile references as its brand value, enabling client-specific branding in the UI

### Requirement 2: Kiro Hook 事件定义

**User Story:** 作为 CodeIsland 用户，我希望 Kiro 的 hook 事件能被正确捕获，以便我能收到完成通知、选择提示和进度更新。

#### Acceptance Criteria

1. THE ManagedHookClientProfile for Kiro SHALL define a "PreToolUse" event with a single template using matcher "*" to capture tool approval prompts for all tool types
2. THE ManagedHookClientProfile for Kiro SHALL define a "PostToolUse" event with a single template using matcher "*" to capture tool completion results for all tool types
3. THE ManagedHookClientProfile for Kiro SHALL define a "Notification" event with a single template using matcher "*" to capture all progress and status notification types
4. THE ManagedHookClientProfile for Kiro SHALL define a "Stop" event with a plain template (no matcher) to capture session completion
5. THE ManagedHookClientProfile for Kiro SHALL define a "UserPromptSubmit" event with a plain template (no matcher) to capture user prompt submissions for progress tracking
6. THE ManagedHookClientProfile for Kiro SHALL define a "SessionStart" event with a plain template (no matcher) to capture session initiation
7. THE ManagedHookClientProfile for Kiro SHALL define a "SessionEnd" event with a plain template (no matcher) to capture session termination
8. THE ManagedHookClientProfile for Kiro SHALL define a "SubagentStop" event with a plain template (no matcher) to capture sub-agent completion
9. THE ManagedHookClientProfile for Kiro SHALL define a "PreCompact" event with two templates using matchers "auto" and "manual" to capture both automatic and user-initiated context compression
10. THE ManagedHookClientProfile for Kiro SHALL define a "PermissionRequest" event with a single template using matcher "*" and a timeout of 86400 seconds to capture tool permission approval prompts

### Requirement 3: Kiro 运行时会话识别

**User Story:** 作为 CodeIsland 用户，我希望来自 Kiro 的会话能被正确识别并显示为 "Kiro" 标签，以便我能在会话列表中区分不同客户端。

#### Acceptance Criteria

1. THE ClientProfileRegistry SHALL include a SessionClientProfile entry with id "kiro" in the runtimeProfiles array, using provider .claude, family .claudeHooks, kind .claudeCode, brand .claude, and assistantLabelMode .badgeLabel
2. THE SessionClientProfile for Kiro SHALL set recognizedKinds to include "kiro", "kiro-ide", and "kiro ide" (case-insensitive after normalization that lowercases and replaces underscores with hyphens)
3. THE SessionClientProfile for Kiro SHALL use "Kiro" as the displayName, and set exactAliases to include at minimum "kiro", "kiro-ide", and "kiro ide" so that originator-based matching yields a score of 60 or above
4. WHEN a hook event arrives with client-kind matching any value in the Kiro profile's recognizedKinds, THE ClientProfileRegistry.matchRuntimeProfile SHALL return the Kiro SessionClientProfile with a matchScore of at least 100
5. WHEN a hook event arrives with client-originator "Kiro" (or any normalized exactAlias match) and no explicit client-kind, THE ClientProfileRegistry.matchRuntimeProfile SHALL return the Kiro SessionClientProfile with a matchScore of at least 60
6. WHEN the Kiro profile is resolved for a session, THE session's assistantLabel SHALL display "Kiro" (the profile's displayName) as the badge label in the session list

### Requirement 4: Kiro 宠物形象集成

**User Story:** 作为 CodeIsland 用户，我希望在设置的宠物页面中看到 Kiro 对应的专属形象，以便我能预览和自定义 Kiro 会话的宠物显示。

#### Acceptance Criteria

1. THE MascotClient enum SHALL include a .kiro case, and the static `allCases` array SHALL include `.kiro` so that it appears in the mascot settings grid
2. THE MascotKind enum SHALL include a .kiro case with a dedicated `drawKiro` rendering method in MascotView that produces a visually distinct mascot from all other existing MascotKind cases
3. THE MascotClient.kiro SHALL have title "Kiro" and a Chinese subtitle referencing Kiro IDE hooks (following the existing pattern where subtitles describe the hook source and mascot character in Chinese)
4. THE MascotClient.kiro SHALL map to MascotKind.kiro as its defaultMascotKind
5. THE MascotKind.kiro SHALL define an alertColor with RGB values that differ from every other MascotKind alertColor by at least 0.05 in at least one channel
6. THE MascotKind.kiro SHALL define a title "Kiro" and a Chinese subtitle that names the mascot character (following the existing pattern of 2–6 character Chinese mascot descriptions)
7. WHEN a session is identified as Kiro via a matching profile ID "kiro" in the MascotClient `init(clientInfo:provider:)` initializer, THE MascotView SHALL render the Kiro mascot animation with distinct visual output for each of the idle, working, and warning states
8. IF MascotClient.kiro is selected in the mascot settings picker, THEN THE MascotSettingsView SHALL display the Kiro mascot preview responding to the idle, working, and warning status toggle

### Requirement 5: Kiro Logo 资源集成

**User Story:** 作为 CodeIsland 用户，我希望在设置和会话列表中看到 Kiro 的官方 logo，以便我能快速识别 Kiro 客户端。

#### Acceptance Criteria

1. THE Assets.xcassets SHALL include a "KiroLogo" imageset containing a single PNG file named "kiro-logo.png" with universal idiom at 1x scale, following the same Contents.json structure as existing logo imagesets (e.g., ClaudeLogo)
2. THE ManagedHookClientProfile for Kiro SHALL reference "KiroLogo" as its logoAssetName
3. THE ManagedHookClientProfile for Kiro SHALL set prefersBundledLogoOverAppIcon to true
4. WHEN Kiro appears in the hook integration settings list, THE settings view SHALL display the KiroLogo asset as the client icon via the SettingsClientIcon component, matching the size and placement used by other client logos
5. WHEN a Kiro session appears in the session list, THE session list view SHALL display the KiroLogo asset as the client identifier icon, consistent with how other client logos are rendered in the list

### Requirement 6: Kiro IDE 扩展配置（终端聚焦）

**User Story:** 作为 CodeIsland 用户，我希望点击 Kiro 会话时能跳转回 Kiro 编辑器的对应终端，以便我能快速切换上下文。

#### Acceptance Criteria

1. THE ClientProfileRegistry SHALL include a ManagedIDEExtensionProfile entry with id "kiro-extension" in the ideExtensionProfiles array, specifying title, subtitle, logoAssetName, iconSymbolName, and localAppBundleIdentifiers fields consistent with the existing profile pattern
2. THE ManagedIDEExtensionProfile for Kiro SHALL specify "kiro" as the uriScheme
3. THE ManagedIDEExtensionProfile for Kiro SHALL specify ".kiro-ide/extensions" as the extensionRootRelativePath
4. WHEN a Kiro session is clicked in the session list, THE TerminalSessionFocuser SHALL construct a URI in the format "kiro://code-island.session-focus/focus" with query parameters identifying the target terminal (pid, tty, sessionId, cwd, or terminalSessionId as available) and open it via NSWorkspace so that the Kiro IDE activates and focuses the matching terminal
5. THE ManagedIDEExtensionProfile for Kiro SHALL include Kiro's bundle identifier in exactBundleIdentifiers so that the ideExtensionProfile(bundleIdentifier:appName:) lookup returns the Kiro profile when the detected host app matches Kiro's bundle identifier
6. IF the Kiro IDE extension is not installed or Kiro is not running when a Kiro session is clicked, THEN THE TerminalSessionFocuser SHALL return false without opening a URI
7. THE ManagedIDEExtensionProfile for Kiro SHALL specify at least one entry in appNameKeywords containing "kiro" (lowercased) so that app-name-based profile resolution can match running Kiro instances

### Requirement 7: Kiro MascotClient 路由集成

**User Story:** 作为 CodeIsland 用户，我希望从 Kiro 发来的 hook 事件能自动关联到正确的宠物形象，以便刘海区和会话列表中显示一致的 Kiro 宠物。

#### Acceptance Criteria

1. WHEN a SessionClientInfo has a resolved profileID equal to "kiro", THE MascotClient initializer SHALL resolve to .kiro, taking precedence over brand-based resolution
2. WHEN a SessionClientInfo has no matching profileID and has brand equal to .kiro, THE MascotClient initializer SHALL resolve to .kiro as a brand-level fallback
3. THE SessionClientBrand enum SHALL include a .kiro case with rawValue "kiro" for Kiro brand identification
4. THE MascotClient.allCases static array SHALL include .kiro so it appears in the mascot settings grid
5. THE MascotClient.kiro case SHALL map its defaultMascotKind to a corresponding .kiro MascotKind case so that the mascot view renders the Kiro pet sprite
