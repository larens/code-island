using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CodeIsland.Windows.Models;

namespace CodeIsland.Windows.Services;

/// <summary>
/// Central state management for all tracked sessions.
/// Uses lock-based concurrency (C# equivalent of Swift's actor model).
/// Corresponds to SessionStore in SessionStore.swift.
/// </summary>
public sealed partial class SessionStore : ObservableObject
{
    private readonly object _lock = new();
    private readonly Dictionary<string, SessionState> _sessions = [];
    private readonly Dictionary<string, NamedPipeConnection> _pendingConnections = [];

    [ObservableProperty]
    private ObservableCollection<SessionState> _activeSessions = [];

    [ObservableProperty]
    private int _attentionCount;

    public event EventHandler<SessionState>? SessionUpdated;
    public event EventHandler<SessionState>? SessionAdded;
    public event EventHandler<string>? SessionRemoved;
    public event EventHandler<(string SessionId, string ToolUseId, string ToolName, string? ToolInput)>? PermissionRequested;

    /// <summary>
    /// Process a session event.
    /// </summary>
    public void Process(SessionEvent evt)
    {
        lock (_lock)
        {
            switch (evt)
            {
                case SessionEvent.HookReceived hookEvent:
                    ProcessHookEvent(hookEvent.HookEvent);
                    break;

                case SessionEvent.PermissionApproved approved:
                    HandlePermissionApproved(approved.SessionId, approved.ToolUseId);
                    break;

                case SessionEvent.PermissionDenied denied:
                    HandlePermissionDenied(denied.SessionId, denied.ToolUseId, denied.Reason);
                    break;

                case SessionEvent.InterventionResolved resolved:
                    HandleInterventionResolved(resolved);
                    break;

                case SessionEvent.ClearSession clear:
                    RemoveSession(clear.SessionId);
                    break;

                case SessionEvent.ArchiveSession archive:
                    ArchiveSession(archive.SessionId);
                    break;
            }
        }
    }

    private void ProcessHookEvent(HookEvent hookEvent)
    {
        var sessionId = hookEvent.SessionId;
        if (string.IsNullOrEmpty(sessionId)) return;

        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            session = CreateSessionFromHook(hookEvent);
            _sessions[sessionId] = session;
            SessionAdded?.Invoke(this, session);
        }
        else
        {
            UpdateSessionFromHook(session, hookEvent);
        }

        session.LastActivity = DateTime.UtcNow;
        SessionUpdated?.Invoke(this, session);
        UpdateActiveSessions();
    }

    private SessionState CreateSessionFromHook(HookEvent hookEvent)
    {
        var projectName = !string.IsNullOrEmpty(hookEvent.Cwd)
            ? Path.GetFileName(hookEvent.Cwd.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            : "Unknown";

        return new SessionState
        {
            SessionId = hookEvent.SessionId,
            Cwd = hookEvent.Cwd,
            ProjectName = projectName,
            Provider = hookEvent.Provider,
            ClientInfo = hookEvent.ClientInfo,
            Ingress = hookEvent.Ingress,
            Pid = hookEvent.Pid,
            Tty = hookEvent.Tty,
            Phase = SessionPhase.Processing
        };
    }

    private void UpdateSessionFromHook(SessionState session, HookEvent hookEvent)
    {
        if (!string.IsNullOrEmpty(hookEvent.Message))
        {
            session.LatestHookMessage = hookEvent.Message;
        }

        // Update phase based on event type
        var eventLower = hookEvent.Event.ToLowerInvariant();
        if (eventLower.Contains("tool_use") || eventLower.Contains("pretooluse"))
        {
            if (!string.IsNullOrEmpty(hookEvent.ToolUseId) && !string.IsNullOrEmpty(hookEvent.Tool))
            {
                session.ToolTracker.TrackTool(hookEvent.ToolUseId, hookEvent.Tool, hookEvent.ToolInput);
            }
        }
        else if (eventLower.Contains("tool_result") || eventLower.Contains("posttooluse"))
        {
            if (!string.IsNullOrEmpty(hookEvent.ToolUseId))
            {
                session.ToolTracker.CompleteTool(hookEvent.ToolUseId, hookEvent.Status != "error");
            }
        }
        else if (eventLower.Contains("stop"))
        {
            session.Phase = SessionPhase.WaitingForInput;
        }
        else if (eventLower.Contains("notification"))
        {
            session.LatestHookMessage = hookEvent.Message;
        }

        // Handle intervention (approval/question)
        // This would be set from the bridge envelope's intervention field
    }

    /// <summary>
    /// Register a pending pipe connection for a permission request.
    /// </summary>
    public void RegisterPendingConnection(string sessionId, string toolUseId, string toolName, string? toolInput, NamedPipeConnection connection)
    {
        lock (_lock)
        {
            var key = $"{sessionId}:{toolUseId}";
            _pendingConnections[key] = connection;

            if (_sessions.TryGetValue(sessionId, out var session))
            {
                session.Phase = SessionPhase.WaitingForApproval(new PermissionContext
                {
                    ToolUseId = toolUseId,
                    ToolName = toolName,
                    ReceivedAt = DateTime.UtcNow
                });
                SessionUpdated?.Invoke(this, session);
                UpdateActiveSessions();
            }

            PermissionRequested?.Invoke(this, (sessionId, toolUseId, toolName, toolInput));
        }
    }

    private void HandlePermissionApproved(string sessionId, string toolUseId)
    {
        var key = $"{sessionId}:{toolUseId}";
        if (_pendingConnections.TryGetValue(key, out var connection))
        {
            var response = new BridgeResponse
            {
                RequestId = connection.RequestId,
                Decision = InterventionDecision.Approve
            };
            connection.SendResponse(response);
            connection.Dispose();
            _pendingConnections.Remove(key);
        }

        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.Phase = SessionPhase.Processing;
            SessionUpdated?.Invoke(this, session);
            UpdateActiveSessions();
        }
    }

    private void HandlePermissionDenied(string sessionId, string toolUseId, string? reason)
    {
        var key = $"{sessionId}:{toolUseId}";
        if (_pendingConnections.TryGetValue(key, out var connection))
        {
            var response = new BridgeResponse
            {
                RequestId = connection.RequestId,
                Decision = InterventionDecision.Deny,
                Reason = reason ?? "Denied by user"
            };
            connection.SendResponse(response);
            connection.Dispose();
            _pendingConnections.Remove(key);
        }

        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.Phase = SessionPhase.Processing;
            SessionUpdated?.Invoke(this, session);
            UpdateActiveSessions();
        }
    }

    private void HandleInterventionResolved(SessionEvent.InterventionResolved resolved)
    {
        if (_sessions.TryGetValue(resolved.SessionId, out var session))
        {
            session.Intervention = null;
            session.Phase = resolved.NextPhase;
            SessionUpdated?.Invoke(this, session);
            UpdateActiveSessions();
        }
    }

    private void RemoveSession(string sessionId)
    {
        if (_sessions.Remove(sessionId))
        {
            SessionRemoved?.Invoke(this, sessionId);
            UpdateActiveSessions();
        }
    }

    private void ArchiveSession(string sessionId)
    {
        RemoveSession(sessionId);
    }

    private void UpdateActiveSessions()
    {
        var sorted = _sessions.Values
            .OrderByDescending(s => s.NeedsAttention)
            .ThenByDescending(s => s.LastActivity)
            .ToList();

        ActiveSessions = new ObservableCollection<SessionState>(sorted);
        AttentionCount = _sessions.Values.Count(s => s.NeedsAttention);
    }

    public SessionState? GetSession(string sessionId)
    {
        lock (_lock)
        {
            return _sessions.TryGetValue(sessionId, out var session) ? session : null;
        }
    }

    public IReadOnlyList<SessionState> GetAllSessions()
    {
        lock (_lock)
        {
            return _sessions.Values.ToList().AsReadOnly();
        }
    }
}

/// <summary>
/// Represents a pending named pipe connection for a permission request.
/// </summary>
public sealed class NamedPipeConnection : IDisposable
{
    public Guid RequestId { get; init; }
    private readonly System.IO.Pipes.NamedPipeServerStream _pipe;

    public NamedPipeConnection(System.IO.Pipes.NamedPipeServerStream pipe, Guid requestId)
    {
        _pipe = pipe;
        RequestId = requestId;
    }

    public void SendResponse(BridgeResponse response)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(response);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            _pipe.Write(bytes, 0, bytes.Length);
            _pipe.Flush();
        }
        catch
        {
            // Connection may have been closed
        }
    }

    public void Dispose()
    {
        try
        {
            _pipe?.Dispose();
        }
        catch { }
    }
}
