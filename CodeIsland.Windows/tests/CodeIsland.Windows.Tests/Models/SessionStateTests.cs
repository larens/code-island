using CodeIsland.Windows.Models;
using FluentAssertions;
using Xunit;

namespace CodeIsland.Windows.Tests.Models;

public class SessionStateTests
{
    [Fact]
    public void NewSession_HasDefaultValues()
    {
        var session = new SessionState
        {
            SessionId = "test-123",
            Cwd = "/home/user/project",
            ProjectName = "project",
            Provider = "claude"
        };

        session.Id.Should().Be("test-123");
        session.Cwd.Should().Be("/home/user/project");
        session.Phase.Kind.Should().Be(SessionPhase.PhaseKind.Idle);
        session.NeedsAttention.Should().BeFalse();
        session.AutoApprovePermissions.Should().BeFalse();
    }

    [Fact]
    public void Session_PhaseChange_UpdatesNeedsAttention()
    {
        var session = new SessionState
        {
            SessionId = "test-123",
            Cwd = "/home/user/project",
            Provider = "claude"
        };

        session.NeedsAttention.Should().BeFalse();

        session.Phase = SessionPhase.WaitingForApproval(new PermissionContext
        {
            ToolUseId = "tool-1",
            ToolName = "Bash"
        });

        session.NeedsAttention.Should().BeTrue();
    }

    [Fact]
    public void ToolTracker_TracksToolExecution()
    {
        var tracker = new ToolTracker();

        tracker.TrackTool("tool-1", "Bash");
        tracker.Tools.Should().ContainKey("tool-1");
        tracker.Tools["tool-1"].Status.Should().Be(ToolStatus.Running);

        tracker.CompleteTool("tool-1", true);
        tracker.Tools["tool-1"].Status.Should().Be(ToolStatus.Completed);
    }

    [Fact]
    public void ToolTracker_RemoveTool()
    {
        var tracker = new ToolTracker();
        tracker.TrackTool("tool-1", "Bash");
        tracker.Tools.Should().ContainKey("tool-1");

        tracker.RemoveTool("tool-1");
        tracker.Tools.Should().NotContainKey("tool-1");
    }

    [Fact]
    public void PermissionContext_FormattedInput_FormatsCorrectly()
    {
        var ctx = new PermissionContext
        {
            ToolUseId = "tool-1",
            ToolName = "Bash",
            ToolInput = new Dictionary<string, object>
            {
                ["command"] = "ls -la",
                ["timeout"] = 30
            }
        };

        var formatted = ctx.FormattedInput;
        formatted.Should().NotBeNull();
        formatted.Should().Contain("command: ls -la");
        formatted.Should().Contain("timeout: 30");
    }
}
