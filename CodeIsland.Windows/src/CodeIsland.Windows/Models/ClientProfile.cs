namespace CodeIsland.Windows.Models;

/// <summary>
/// Hook installation kind.
/// Corresponds to ManagedHookInstallationKind in ClientProfile.swift.
/// </summary>
public enum ManagedHookInstallationKind
{
    JsonHooks,
    PluginFile,
    PluginDirectory,
    HookDirectory
}

/// <summary>
/// Hook install event descriptor.
/// Corresponds to HookInstallEventDescriptor in ClientProfile.swift.
/// </summary>
public sealed class HookInstallEventDescriptor
{
    public string Name { get; init; } = string.Empty;
    public List<string> Templates { get; init; } = [];
    public int? Timeout { get; init; }
}

/// <summary>
/// Client profile defining a supported AI coding tool.
/// Corresponds to ManagedHookClientProfile in ClientProfile.swift.
/// </summary>
public sealed class ManagedHookClientProfile
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public ManagedHookInstallationKind InstallationKind { get; init; } = ManagedHookInstallationKind.JsonHooks;
    public bool AlwaysVisibleInSettings { get; init; }
    public string? LogoAssetName { get; init; }
    public bool PrefersBundledLogoOverAppIcon { get; init; }
    public List<string> LocalAppBundleIdentifiers { get; init; } = [];
    public string IconSymbolName { get; init; } = string.Empty;
    public string ConfigurationRelativePath { get; init; } = string.Empty;
    public List<string> AdditionalConfigurationPaths { get; init; } = [];
    public string? ActivationConfigurationRelativePath { get; init; }
    public string? ActivationEntryName { get; init; }
    public string BridgeSource { get; init; } = string.Empty;
    public List<string> BridgeExtraArguments { get; init; } = [];
    public bool DefaultEnabled { get; init; }
    public SessionClientBrand Brand { get; init; }
    public List<HookInstallEventDescriptor> Events { get; init; } = [];

    /// <summary>
    /// Windows-specific configuration paths for this tool.
    /// </summary>
    public List<string> WindowsConfigurationPaths { get; init; } = [];

    /// <summary>
    /// Windows process names for detection.
    /// </summary>
    public List<string> WindowsProcessNames { get; init; } = [];
}

/// <summary>
/// Registry of all supported AI coding tools.
/// </summary>
public static class ClientProfileRegistry
{
    public static readonly List<ManagedHookClientProfile> Profiles =
    [
        new()
        {
            Id = "claude-code",
            Title = "Claude Code",
            Subtitle = "Anthropic CLI Agent",
            Brand = SessionClientBrand.Claude,
            DefaultEnabled = true,
            IconSymbolName = "chevron.left.forwardslash.chevron.right",
            ConfigurationRelativePath = ".claude/settings.json",
            WindowsConfigurationPaths = [
                "%USERPROFILE%\\.claude\\settings.json",
                "%APPDATA%\\Claude\\settings.json"
            ],
            WindowsProcessNames = ["claude"],
            BridgeSource = "claude",
            Events = [
                new() { Name = "pre_tool_use", Templates = ["pre-tool-use"] },
                new() { Name = "post_tool_use", Templates = ["post-tool-use"] },
                new() { Name = "notification", Templates = ["notification"] },
                new() { Name = "stop", Templates = ["stop"] }
            ]
        },
        new()
        {
            Id = "codex",
            Title = "Codex",
            Subtitle = "OpenAI CLI Agent",
            Brand = SessionClientBrand.Codex,
            DefaultEnabled = true,
            IconSymbolName = "command",
            ConfigurationRelativePath = ".codex/config.json",
            WindowsConfigurationPaths = [
                "%USERPROFILE%\\.codex\\config.json",
                "%APPDATA%\\Codex\\config.json"
            ],
            WindowsProcessNames = ["codex"],
            BridgeSource = "codex",
            InstallationKind = ManagedHookInstallationKind.PluginFile,
            Events = [
                new() { Name = "agent_event", Templates = ["agent-event"] }
            ]
        },
        new()
        {
            Id = "gemini-cli",
            Title = "Gemini CLI",
            Subtitle = "Google AI Agent",
            Brand = SessionClientBrand.Gemini,
            DefaultEnabled = true,
            IconSymbolName = "sparkles",
            ConfigurationRelativePath = ".gemini/settings.json",
            WindowsConfigurationPaths = [
                "%USERPROFILE%\\.gemini\\settings.json"
            ],
            WindowsProcessNames = ["gemini"],
            BridgeSource = "gemini",
            Events = [
                new() { Name = "event", Templates = ["event"] }
            ]
        },
        new()
        {
            Id = "hermes-agent",
            Title = "Hermes Agent",
            Subtitle = "Hermes CLI Agent",
            Brand = SessionClientBrand.Hermes,
            DefaultEnabled = false,
            IconSymbolName = "flame",
            ConfigurationRelativePath = ".hermes/config.json",
            WindowsConfigurationPaths = [
                "%USERPROFILE%\\.hermes\\config.json"
            ],
            WindowsProcessNames = ["hermes"],
            BridgeSource = "hermes",
            Events = [
                new() { Name = "event", Templates = ["event"] }
            ]
        },
        new()
        {
            Id = "qwen-code",
            Title = "Qwen Code",
            Subtitle = "Alibaba AI Agent",
            Brand = SessionClientBrand.Qwen,
            DefaultEnabled = false,
            IconSymbolName = "text.bubble",
            ConfigurationRelativePath = ".qwen/settings.json",
            WindowsConfigurationPaths = [
                "%USERPROFILE%\\.qwen\\settings.json"
            ],
            WindowsProcessNames = ["qwen"],
            BridgeSource = "qwen",
            Events = [
                new() { Name = "event", Templates = ["event"] }
            ]
        },
        new()
        {
            Id = "opencode",
            Title = "OpenCode",
            Subtitle = "Open Code Agent",
            Brand = SessionClientBrand.OpenCode,
            DefaultEnabled = false,
            IconSymbolName = "chevron.left.forwardslash.chevron.right",
            ConfigurationRelativePath = ".opencode/config.json",
            WindowsConfigurationPaths = [
                "%USERPROFILE%\\.opencode\\config.json"
            ],
            WindowsProcessNames = ["opencode"],
            BridgeSource = "opencode",
            Events = [
                new() { Name = "event", Templates = ["event"] }
            ]
        },
        new()
        {
            Id = "copilot",
            Title = "GitHub Copilot",
            Subtitle = "GitHub AI Agent",
            Brand = SessionClientBrand.Copilot,
            DefaultEnabled = false,
            IconSymbolName = "person.fill",
            ConfigurationRelativePath = ".copilot/config.json",
            WindowsConfigurationPaths = [
                "%APPDATA%\\GitHub Copilot\\config.json"
            ],
            WindowsProcessNames = ["copilot"],
            BridgeSource = "copilot",
            Events = [
                new() { Name = "event", Templates = ["event"] }
            ]
        }
    ];
}
