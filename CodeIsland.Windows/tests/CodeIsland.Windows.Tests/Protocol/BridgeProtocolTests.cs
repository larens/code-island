using System.Text.Json;
using CodeIsland.Windows.Models;
using FluentAssertions;
using Xunit;

namespace CodeIsland.Windows.Tests.Protocol;

/// <summary>
/// Tests to verify JSON wire compatibility between C# and Swift bridge protocol.
/// </summary>
public class BridgeProtocolTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void BridgeEnvelope_SerializesToJson_MatchesExpectedFormat()
    {
        // Arrange
        var envelope = new BridgeEnvelope
        {
            Id = Guid.Parse("12345678-1234-1234-1234-123456789abc"),
            Provider = AgentProvider.Claude,
            EventType = "pre_tool_use",
            SessionKey = "session-abc-123",
            Title = "Tool Use Request",
            Preview = "Claude wants to use Bash",
            Cwd = "/home/user/project",
            Status = new BridgeSessionStatus
            {
                Kind = SessionStatusKind.WaitingForApproval
            },
            TerminalContext = new TerminalContext
            {
                TerminalProgram = "iTerm2",
                Tty = "/dev/ttys001"
            },
            ExpectsResponse = true,
            Metadata = new Dictionary<string, string>
            {
                ["tool"] = "Bash",
                ["toolUseId"] = "tool-xyz-789"
            },
            SentAt = DateTime.SpecifyKind(new DateTime(2024, 1, 15, 10, 30, 0), DateTimeKind.Utc)
        };

        // Act
        var json = JsonSerializer.Serialize(envelope, JsonOptions);

        // Assert
        json.Should().Contain("\"provider\":\"claude\"");
        json.Should().Contain("\"eventType\":\"pre_tool_use\"");
        json.Should().Contain("\"sessionKey\":\"session-abc-123\"");
        json.Should().Contain("\"expectsResponse\":true");
        json.Should().Contain("\"tool\":\"Bash\"");
        json.Should().Contain("\"toolUseId\":\"tool-xyz-789\"");
    }

    [Fact]
    public void BridgeEnvelope_DeserializesFromJson_MatchesSwiftFormat()
    {
        // Simulate a JSON payload from the Swift bridge
        var json = """
        {
            "id": "12345678-1234-1234-1234-123456789abc",
            "provider": "claude",
            "eventType": "pre_tool_use",
            "sessionKey": "session-abc-123",
            "title": "Tool Use Request",
            "preview": "Claude wants to use Bash",
            "cwd": "/home/user/project",
            "status": {
                "kind": "waitingForApproval",
                "detail": null
            },
            "terminalContext": {
                "terminalProgram": "iTerm2",
                "tty": "/dev/ttys001"
            },
            "intervention": null,
            "expectsResponse": true,
            "metadata": {
                "tool": "Bash",
                "toolUseId": "tool-xyz-789"
            },
            "sentAt": "2024-01-15T10:30:00Z"
        }
        """;

        // Act
        var envelope = JsonSerializer.Deserialize<BridgeEnvelope>(json, JsonOptions);

        // Assert
        envelope.Should().NotBeNull();
        envelope!.Provider.Should().Be(AgentProvider.Claude);
        envelope.EventType.Should().Be("pre_tool_use");
        envelope.SessionKey.Should().Be("session-abc-123");
        envelope.ExpectsResponse.Should().BeTrue();
        envelope.Status.Should().NotBeNull();
        envelope.Status!.Kind.Should().Be(SessionStatusKind.WaitingForApproval);
        envelope.TerminalContext.TerminalProgram.Should().Be("iTerm2");
        envelope.Metadata.Should().ContainKey("tool");
        envelope.Metadata["tool"].Should().Be("Bash");
    }

    [Fact]
    public void BridgeResponse_SerializesToJson_MatchesExpectedFormat()
    {
        // Arrange
        var response = new BridgeResponse
        {
            RequestId = Guid.Parse("12345678-1234-1234-1234-123456789abc"),
            Decision = InterventionDecision.Approve
        };

        // Act
        var json = JsonSerializer.Serialize(response, JsonOptions);

        // Assert
        json.Should().Contain("\"requestID\":");
        json.Should().Contain("\"decision\":\"approve\"");
    }

    [Fact]
    public void BridgeResponse_DenyDecision_SerializesCorrectly()
    {
        var response = new BridgeResponse
        {
            RequestId = Guid.NewGuid(),
            Decision = InterventionDecision.Deny,
            Reason = "Not allowed"
        };

        var json = JsonSerializer.Serialize(response, JsonOptions);

        json.Should().Contain("\"decision\":\"deny\"");
        json.Should().Contain("\"reason\":\"Not allowed\"");
    }

    [Fact]
    public void InterventionDecision_AllValues_SerializeCorrectly()
    {
        var decisions = new[]
        {
            (InterventionDecision.Approve, "\"approve\""),
            (InterventionDecision.ApproveForSession, "\"approveForSession\""),
            (InterventionDecision.Deny, "\"deny\""),
            (InterventionDecision.Cancel, "\"cancel\"")
        };

        foreach (var (decision, expected) in decisions)
        {
            var response = new BridgeResponse
            {
                RequestId = Guid.NewGuid(),
                Decision = decision
            };

            var json = JsonSerializer.Serialize(response, JsonOptions);
            json.Should().Contain(expected);
        }
    }

    [Fact]
    public void JsonValue_NullValue_SerializesAsNull()
    {
        var json = JsonSerializer.Serialize(JsonValue.Null);
        json.Should().Be("null");
    }

    [Fact]
    public void JsonValue_ComplexObject_SerializesCorrectly()
    {
        var obj = JsonValue.Object(new Dictionary<string, JsonValue>
        {
            ["name"] = JsonValue.String("test"),
            ["count"] = JsonValue.Int(42),
            ["active"] = JsonValue.Bool(true),
            ["nested"] = JsonValue.Object(new Dictionary<string, JsonValue>
            {
                ["key"] = JsonValue.String("value")
            })
        });

        var json = JsonSerializer.Serialize(obj);
        json.Should().Contain("\"name\":\"test\"");
        json.Should().Contain("\"count\":42");
        json.Should().Contain("\"active\":true");
        json.Should().Contain("\"nested\"");
    }

    [Fact]
    public void SessionStatusKind_AllValues_DeserializeCorrectly()
    {
        var kinds = new[]
        {
            ("idle", SessionStatusKind.Idle),
            ("active", SessionStatusKind.Active),
            ("thinking", SessionStatusKind.Thinking),
            ("runningTool", SessionStatusKind.RunningTool),
            ("waitingForApproval", SessionStatusKind.WaitingForApproval),
            ("waitingForInput", SessionStatusKind.WaitingForInput),
            ("compacting", SessionStatusKind.Compacting),
            ("completed", SessionStatusKind.Completed),
            ("interrupted", SessionStatusKind.Interrupted),
            ("notification", SessionStatusKind.Notification),
            ("error", SessionStatusKind.Error)
        };

        foreach (var (value, expected) in kinds)
        {
            var json = $"\"{value}\"";
            var result = JsonSerializer.Deserialize<SessionStatusKind>(json);
            result.Should().Be(expected);
        }
    }
}
