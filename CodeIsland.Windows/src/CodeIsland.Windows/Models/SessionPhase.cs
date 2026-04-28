using System.Text.Json.Serialization;

namespace CodeIsland.Windows.Models;

/// <summary>
/// Permission context for tools waiting for approval.
/// Corresponds to PermissionContext in SessionPhase.swift.
/// </summary>
public sealed class PermissionContext
{
    public string ToolUseId { get; init; } = string.Empty;
    public string ToolName { get; init; } = string.Empty;
    public Dictionary<string, object>? ToolInput { get; init; }
    public DateTime ReceivedAt { get; init; } = DateTime.UtcNow;

    public string? FormattedInput
    {
        get
        {
            if (ToolInput == null || ToolInput.Count == 0) return null;

            var parts = new List<string>();
            foreach (var kvp in ToolInput.OrderBy(k => k.Key))
            {
                var valueStr = kvp.Value switch
                {
                    string s => s.Length > 100 ? string.Concat(s.AsSpan(0, 100), "...") : s,
                    int i => i.ToString(),
                    double d => d.ToString(),
                    bool b => b ? "true" : "false",
                    _ => "..."
                };
                parts.Add($"{kvp.Key}: {valueStr}");
            }
            return string.Join("\n", parts);
        }
    }
}

/// <summary>
/// Explicit session phases - the state machine.
/// Corresponds to SessionPhase in SessionPhase.swift.
/// </summary>
[JsonConverter(typeof(SessionPhaseJsonConverter))]
public sealed class SessionPhase : IEquatable<SessionPhase>
{
    public enum PhaseKind
    {
        Idle,
        Processing,
        WaitingForInput,
        WaitingForApproval,
        Compacting,
        Ended
    }

    public PhaseKind Kind { get; }
    public PermissionContext? PermissionCtx { get; }

    private SessionPhase(PhaseKind kind, PermissionContext? permissionCtx = null)
    {
        Kind = kind;
        PermissionCtx = permissionCtx;
    }

    public static SessionPhase Idle => new(PhaseKind.Idle);
    public static SessionPhase Processing => new(PhaseKind.Processing);
    public static SessionPhase WaitingForInput => new(PhaseKind.WaitingForInput);
    public static SessionPhase WaitingForApproval(PermissionContext ctx) => new(PhaseKind.WaitingForApproval, ctx);
    public static SessionPhase Compacting => new(PhaseKind.Compacting);
    public static SessionPhase Ended => new(PhaseKind.Ended);

    public bool NeedsAttention => Kind is PhaseKind.WaitingForApproval or PhaseKind.WaitingForInput;

    public bool CanTransitionTo(SessionPhase next)
    {
        // Terminal state - no transitions out
        if (Kind == PhaseKind.Ended) return false;

        return true;
    }

    public bool Equals(SessionPhase? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Kind == other.Kind && Equals(PermissionCtx, other.PermissionCtx);
    }

    public override bool Equals(object? obj) => Equals(obj as SessionPhase);
    public override int GetHashCode() => HashCode.Combine(Kind, PermissionCtx);
}

/// <summary>
/// JSON converter for SessionPhase to support polymorphic serialization.
/// </summary>
public sealed class SessionPhaseJsonConverter : JsonConverter<SessionPhase>
{
    public override SessionPhase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var kind = JsonSerializer.Deserialize<SessionPhase.PhaseKind>(ref reader, options);
        return kind switch
        {
            SessionPhase.PhaseKind.Idle => SessionPhase.Idle,
            SessionPhase.PhaseKind.Processing => SessionPhase.Processing,
            SessionPhase.PhaseKind.WaitingForInput => SessionPhase.WaitingForInput,
            SessionPhase.PhaseKind.Compacting => SessionPhase.Compacting,
            SessionPhase.PhaseKind.Ended => SessionPhase.Ended,
            _ => SessionPhase.Idle
        };
    }

    public override void Write(Utf8JsonWriter writer, SessionPhase value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Kind, options);
    }
}
