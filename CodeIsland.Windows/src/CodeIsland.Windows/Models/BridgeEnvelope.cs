using System.Text.Json.Serialization;

namespace CodeIsland.Windows.Models;

/// <summary>
/// Agent provider types supported by the bridge.
/// Corresponds to AgentProvider in IslandShared/Models.swift.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AgentProvider
{
    [JsonPropertyName("claude")]
    Claude,
    [JsonPropertyName("codex")]
    Codex,
    [JsonPropertyName("copilot")]
    Copilot
}

/// <summary>
/// Session status kind from the bridge.
/// Corresponds to SessionStatusKind in IslandShared/Models.swift.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SessionStatusKind
{
    [JsonPropertyName("idle")]
    Idle,
    [JsonPropertyName("active")]
    Active,
    [JsonPropertyName("thinking")]
    Thinking,
    [JsonPropertyName("runningTool")]
    RunningTool,
    [JsonPropertyName("waitingForApproval")]
    WaitingForApproval,
    [JsonPropertyName("waitingForInput")]
    WaitingForInput,
    [JsonPropertyName("compacting")]
    Compacting,
    [JsonPropertyName("completed")]
    Completed,
    [JsonPropertyName("interrupted")]
    Interrupted,
    [JsonPropertyName("notification")]
    Notification,
    [JsonPropertyName("error")]
    Error
}

/// <summary>
/// Session status from the bridge.
/// Corresponds to SessionStatus in IslandShared/Models.swift.
/// </summary>
public sealed class BridgeSessionStatus
{
    [JsonPropertyName("kind")]
    public SessionStatusKind Kind { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonIgnore]
    public bool RequiresAttention => Kind is SessionStatusKind.WaitingForApproval
        or SessionStatusKind.WaitingForInput
        or SessionStatusKind.Error;
}

/// <summary>
/// Terminal context information.
/// Corresponds to TerminalContext in IslandShared/Models.swift.
/// </summary>
public sealed class TerminalContext
{
    [JsonPropertyName("terminalProgram")]
    public string? TerminalProgram { get; set; }

    [JsonPropertyName("terminalBundleID")]
    public string? TerminalBundleId { get; set; }

    [JsonPropertyName("ideName")]
    public string? IdeName { get; set; }

    [JsonPropertyName("ideBundleID")]
    public string? IdeBundleId { get; set; }

    [JsonPropertyName("iTermSessionID")]
    public string? ITermSessionId { get; set; }

    [JsonPropertyName("terminalSessionID")]
    public string? TerminalSessionId { get; set; }

    [JsonPropertyName("tty")]
    public string? Tty { get; set; }

    [JsonPropertyName("currentDirectory")]
    public string? CurrentDirectory { get; set; }

    [JsonPropertyName("transport")]
    public string? Transport { get; set; }

    [JsonPropertyName("remoteHost")]
    public string? RemoteHost { get; set; }

    [JsonPropertyName("tmuxSession")]
    public string? TmuxSession { get; set; }

    [JsonPropertyName("tmuxPane")]
    public string? TmuxPane { get; set; }
}

/// <summary>
/// Intervention kind from the bridge.
/// Corresponds to InterventionKind in IslandShared/Models.swift.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InterventionKind
{
    [JsonPropertyName("approval")]
    Approval,
    [JsonPropertyName("question")]
    Question
}

/// <summary>
/// Intervention option.
/// Corresponds to InterventionOption in IslandShared/Models.swift.
/// </summary>
public sealed class InterventionOption
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }
}

/// <summary>
/// Intervention request from the bridge.
/// Corresponds to InterventionRequest in IslandShared/Models.swift.
/// </summary>
public sealed class InterventionRequest
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyName("sessionID")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("kind")]
    public InterventionKind Kind { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("options")]
    public List<InterventionOption> Options { get; set; } = [];

    [JsonPropertyName("rawContext")]
    public Dictionary<string, string> RawContext { get; set; } = [];
}

/// <summary>
/// Bridge envelope sent from CodeIslandBridge to the app.
/// Corresponds to BridgeEnvelope in IslandShared/Models.swift:175.
/// JSON format must be wire-compatible with the Swift version.
/// </summary>
public sealed class BridgeEnvelope
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyName("provider")]
    public AgentProvider Provider { get; set; }

    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;

    [JsonPropertyName("sessionKey")]
    public string SessionKey { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("preview")]
    public string? Preview { get; set; }

    [JsonPropertyName("cwd")]
    public string? Cwd { get; set; }

    [JsonPropertyName("status")]
    public BridgeSessionStatus? Status { get; set; }

    [JsonPropertyName("terminalContext")]
    public TerminalContext TerminalContext { get; set; } = new();

    [JsonPropertyName("intervention")]
    public InterventionRequest? Intervention { get; set; }

    [JsonPropertyName("expectsResponse")]
    public bool ExpectsResponse { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = [];

    [JsonPropertyName("sentAt")]
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
