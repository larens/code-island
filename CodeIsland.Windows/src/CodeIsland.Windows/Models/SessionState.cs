using CommunityToolkit.Mvvm.ComponentModel;

namespace CodeIsland.Windows.Models;

/// <summary>
/// Client brand identifiers.
/// Corresponds to SessionClientBrand in ClientProfile.swift.
/// </summary>
public enum SessionClientBrand
{
    Claude,
    CodeBuddy,
    Codex,
    Gemini,
    Hermes,
    Qwen,
    OpenCode,
    Qoder,
    Copilot,
    Neutral
}

/// <summary>
/// Hook protocol family.
/// Corresponds to HookProtocolFamily in ClientProfile.swift.
/// </summary>
public enum HookProtocolFamily
{
    ClaudeHooks,
    CodexHooks,
    CodexAppServer
}

/// <summary>
/// Session ingress type.
/// </summary>
public enum SessionIngress
{
    HookBridge,
    AppServer,
    NativeRuntime
}

/// <summary>
/// Client information for a session.
/// </summary>
public sealed class SessionClientInfo
{
    public string Brand { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Version { get; set; }

    public static SessionClientInfo Default(string brand) => new()
    {
        Brand = brand,
        DisplayName = brand
    };
}

/// <summary>
/// Tool tracker for monitoring tool execution state.
/// Simplified from the Swift ToolTracker.
/// </summary>
public sealed class ToolTracker
{
    private readonly object _lock = new();
    private readonly Dictionary<string, ToolInfo> _tools = [];

    public IReadOnlyDictionary<string, ToolInfo> Tools
    {
        get { lock (_lock) return _tools.ToDictionary(kvp => kvp.Key, kvp => kvp.Value); }
    }

    public void TrackTool(string toolUseId, string toolName, Dictionary<string, object>? input = null)
    {
        lock (_lock)
        {
            _tools[toolUseId] = new ToolInfo
            {
                ToolUseId = toolUseId,
                ToolName = toolName,
                Input = input,
                Status = ToolStatus.Running,
                StartedAt = DateTime.UtcNow
            };
        }
    }

    public void CompleteTool(string toolUseId, bool success)
    {
        lock (_lock)
        {
            if (_tools.TryGetValue(toolUseId, out var tool))
            {
                _tools[toolUseId] = tool with
                {
                    Status = success ? ToolStatus.Completed : ToolStatus.Error,
                    CompletedAt = DateTime.UtcNow
                };
            }
        }
    }

    public void RemoveTool(string toolUseId)
    {
        lock (_lock)
        {
            _tools.Remove(toolUseId);
        }
    }
}

public enum ToolStatus
{
    Running,
    Completed,
    Error
}

public sealed record ToolInfo
{
    public string ToolUseId { get; init; } = string.Empty;
    public string ToolName { get; init; } = string.Empty;
    public Dictionary<string, object>? Input { get; init; }
    public ToolStatus Status { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

/// <summary>
/// Complete state for a single tracked session.
/// Corresponds to SessionState in SessionState.swift.
/// This is the single source of truth - all state reads and writes go through SessionStore.
/// </summary>
public sealed partial class SessionState : ObservableObject
{
    [ObservableProperty]
    private string _sessionId = string.Empty;

    [ObservableProperty]
    private string _cwd = string.Empty;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _provider = "claude";

    [ObservableProperty]
    private SessionClientInfo _clientInfo = new();

    [ObservableProperty]
    private SessionIngress _ingress = SessionIngress.HookBridge;

    [ObservableProperty]
    private string? _sessionName;

    [ObservableProperty]
    private string? _previewText;

    [ObservableProperty]
    private string? _latestHookMessage;

    [ObservableProperty]
    private InterventionRequest? _intervention;

    [ObservableProperty]
    private int? _pid;

    [ObservableProperty]
    private string? _tty;

    [ObservableProperty]
    private bool _isInTmux;

    [ObservableProperty]
    private bool _autoApprovePermissions;

    [ObservableProperty]
    private SessionPhase _phase = SessionPhase.Idle;

    [ObservableProperty]
    private ToolTracker _toolTracker = new();

    [ObservableProperty]
    private DateTime _lastActivity = DateTime.UtcNow;

    [ObservableProperty]
    private DateTime _createdAt = DateTime.UtcNow;

    // Derived properties
    public bool NeedsAttention => Phase.NeedsAttention || Intervention != null;
    public bool NeedsManualAttention => NeedsAttention;
    public string Id => SessionId;
}
