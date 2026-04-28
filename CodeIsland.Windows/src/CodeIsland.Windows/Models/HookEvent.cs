namespace CodeIsland.Windows.Models;

/// <summary>
/// Event received from hook clients after bridge-envelope mapping.
/// Corresponds to HookEvent in HookSocketServer.swift.
/// </summary>
public sealed class HookEvent
{
    public string SessionId { get; init; } = string.Empty;
    public string Cwd { get; init; } = string.Empty;
    public string Event { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public SessionClientInfo ClientInfo { get; init; } = new();
    public int? Pid { get; init; }
    public string? Tty { get; init; }
    public string? Tool { get; init; }
    public Dictionary<string, object>? ToolInput { get; init; }
    public string? ToolUseId { get; init; }
    public string? NotificationType { get; init; }
    public string? Message { get; init; }
    public SessionIngress Ingress { get; init; } = SessionIngress.HookBridge;

    /// <summary>
    /// Create a HookEvent from a BridgeEnvelope.
    /// </summary>
    public static HookEvent FromBridgeEnvelope(BridgeEnvelope envelope)
    {
        return new HookEvent
        {
            SessionId = envelope.SessionKey,
            Cwd = envelope.Cwd ?? string.Empty,
            Event = envelope.EventType,
            Status = envelope.Status?.Kind.ToString() ?? "active",
            Provider = envelope.Provider.ToString().ToLowerInvariant(),
            ClientInfo = SessionClientInfo.Default(envelope.Provider.ToString()),
            Tool = envelope.Metadata.TryGetValue("tool", out var tool) ? tool : null,
            ToolUseId = envelope.Metadata.TryGetValue("toolUseId", out var toolUseId) ? toolUseId : null,
            NotificationType = envelope.Metadata.TryGetValue("notificationType", out var nt) ? nt : null,
            Message = envelope.Title ?? envelope.Preview,
            Ingress = SessionIngress.HookBridge
        };
    }
}
