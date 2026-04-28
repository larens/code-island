using CodeIsland.Windows.Models;
using FluentAssertions;
using Xunit;

namespace CodeIsland.Windows.Tests.Models;

public class SessionPhaseTests
{
    [Fact]
    public void Idle_Phase_HasCorrectKind()
    {
        var phase = SessionPhase.Idle;
        phase.Kind.Should().Be(SessionPhase.PhaseKind.Idle);
        phase.NeedsAttention.Should().BeFalse();
    }

    [Fact]
    public void Processing_Phase_HasCorrectKind()
    {
        var phase = SessionPhase.Processing;
        phase.Kind.Should().Be(SessionPhase.PhaseKind.Processing);
        phase.NeedsAttention.Should().BeFalse();
    }

    [Fact]
    public void WaitingForApproval_Phase_NeedsAttention()
    {
        var ctx = new PermissionContext
        {
            ToolUseId = "tool-1",
            ToolName = "Bash",
            ReceivedAt = DateTime.UtcNow
        };
        var phase = SessionPhase.WaitingForApproval(ctx);

        phase.Kind.Should().Be(SessionPhase.PhaseKind.WaitingForApproval);
        phase.NeedsAttention.Should().BeTrue();
        phase.PermissionCtx.Should().NotBeNull();
        phase.PermissionCtx!.ToolUseId.Should().Be("tool-1");
    }

    [Fact]
    public void WaitingForInput_Phase_NeedsAttention()
    {
        var phase = SessionPhase.WaitingForInput;
        phase.Kind.Should().Be(SessionPhase.PhaseKind.WaitingForInput);
        phase.NeedsAttention.Should().BeTrue();
    }

    [Fact]
    public void Ended_Phase_CannotTransition()
    {
        var phase = SessionPhase.Ended;
        phase.CanTransitionTo(SessionPhase.Idle).Should().BeFalse();
        phase.CanTransitionTo(SessionPhase.Processing).Should().BeFalse();
    }

    [Fact]
    public void Idle_Phase_CanTransitionToProcessing()
    {
        var phase = SessionPhase.Idle;
        phase.CanTransitionTo(SessionPhase.Processing).Should().BeTrue();
    }

    [Fact]
    public void SessionPhase_Equality_Works()
    {
        var phase1 = SessionPhase.Idle;
        var phase2 = SessionPhase.Idle;
        var phase3 = SessionPhase.Processing;

        phase1.Should().Be(phase2);
        phase1.Should().NotBe(phase3);
    }
}
