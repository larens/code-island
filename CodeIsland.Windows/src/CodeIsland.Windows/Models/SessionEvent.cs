namespace CodeIsland.Windows.Models;

/// <summary>
/// All events that can affect session state.
/// This is the single entry point for state mutations.
/// Corresponds to SessionEvent in SessionEvent.swift.
/// </summary>
public abstract record SessionEvent
{
    // Hook Events (from NamedPipeServer)
    public record HookReceived(HookEvent HookEvent) : SessionEvent;

    // Permission Events (user actions)
    public record PermissionApproved(string SessionId, string ToolUseId) : SessionEvent;
    public record PermissionAutoApprovalChanged(string SessionId, bool IsEnabled) : SessionEvent;
    public record PermissionDenied(string SessionId, string ToolUseId, string? Reason) : SessionEvent;
    public record PermissionSocketFailed(string SessionId, string ToolUseId) : SessionEvent;
    public record InterventionResolved(string SessionId, SessionPhase NextPhase, Dictionary<string, List<string>>? SubmittedAnswers = null) : SessionEvent;

    // File Events (from JSONL parsing)
    public record FileUpdated(FileUpdatePayload Payload) : SessionEvent;

    // Tool Completion Events (from JSONL parsing)
    public record ToolCompleted(string SessionId, string ToolUseId, ToolCompletionResult Result) : SessionEvent;

    // Interrupt Events
    public record InterruptDetected(string SessionId) : SessionEvent;

    // Subagent Events
    public record SubagentStarted(string SessionId, string TaskToolId) : SessionEvent;
    public record SubagentToolExecuted(string SessionId, SubagentToolCall Tool) : SessionEvent;
    public record SubagentToolCompleted(string SessionId, string ToolId, ToolStatus Status) : SessionEvent;
    public record SubagentStopped(string SessionId, string TaskToolId) : SessionEvent;

    // Clear Events
    public record ClearSession(string SessionId) : SessionEvent;
    public record ArchiveSession(string SessionId) : SessionEvent;
}

/// <summary>
/// File update payload from JSONL parsing.
/// </summary>
public sealed class FileUpdatePayload
{
    public string SessionId { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string? LastLine { get; init; }
}

/// <summary>
/// Tool completion result.
/// </summary>
public sealed class ToolCompletionResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? Output { get; init; }
}

/// <summary>
/// Subagent tool call info.
/// </summary>
public sealed class SubagentToolCall
{
    public string ToolId { get; init; } = string.Empty;
    public string ToolName { get; init; } = string.Empty;
    public Dictionary<string, object>? Input { get; init; }
}

/// <summary>
/// Subagent tool info from agent file.
/// </summary>
public sealed class SubagentToolInfo
{
    public string ToolId { get; init; } = string.Empty;
    public string ToolName { get; init; } = string.Empty;
    public ToolStatus Status { get; init; }
}
