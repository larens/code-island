using System.IO.Pipes;
using System.Text.Json;
using CodeIsland.Windows.Models;

namespace CodeIsland.Windows.Services;

/// <summary>
/// Named Pipe server for receiving bridge events.
/// Replaces the Unix domain socket server from HookSocketServer.swift.
/// Listens on \\.\pipe\codeisland.
/// </summary>
public sealed class NamedPipeServer
{
    private const string PipeName = "codeisland";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly SessionStore _sessionStore;
    private readonly CancellationTokenSource _cts = new();
    private readonly List<Task> _listenerTasks = [];
    private bool _running;

    public NamedPipeServer(SessionStore sessionStore)
    {
        _sessionStore = sessionStore;
    }

    public Task StartAsync()
    {
        if (_running) return Task.CompletedTask;
        _running = true;

        var listenerTask = Task.Run(() => ListenLoop(_cts.Token));
        _listenerTasks.Add(listenerTask);

        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _running = false;
        _cts.Cancel();

        try
        {
            await Task.WhenAll(_listenerTasks);
        }
        catch (OperationCanceledException) { }
    }

    private async Task ListenLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _running)
        {
            try
            {
                var pipeServer = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await pipeServer.WaitForConnectionAsync(ct);

                // Handle each connection in a separate task
                _ = Task.Run(() => HandleConnection(pipeServer, ct), ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // Brief delay before retrying to avoid tight loop on persistent errors
                try
                {
                    await Task.Delay(1000, ct);
                }
                catch (OperationCanceledException) { break; }
            }
        }
    }

    private async Task HandleConnection(NamedPipeServerStream pipe, CancellationToken ct)
    {
        try
        {
            using var reader = new StreamReader(pipe, leaveOpen: true);
            var json = await reader.ReadLineAsync(ct);

            if (string.IsNullOrEmpty(json)) return;

            var envelope = JsonSerializer.Deserialize<BridgeEnvelope>(json, JsonOptions);
            if (envelope == null) return;

            var hookEvent = HookEvent.FromBridgeEnvelope(envelope);

            if (envelope.ExpectsResponse)
            {
                // Keep the connection open for the permission response
                var connection = new NamedPipeConnection(pipe, envelope.Id);
                _sessionStore.RegisterPendingConnection(
                    envelope.SessionKey,
                    envelope.Metadata.TryGetValue("toolUseId", out var toolUseId) ? toolUseId : string.Empty,
                    envelope.Metadata.TryGetValue("tool", out var tool) ? tool : "unknown",
                    envelope.Preview,
                    connection);

                // Don't dispose - the connection will be disposed when the response is sent
                return;
            }

            _sessionStore.Process(new SessionEvent.HookReceived(hookEvent));

            // Send acknowledgment for non-response requests
            var ackResponse = new BridgeResponse
            {
                RequestId = envelope.Id
            };
            var ackJson = JsonSerializer.Serialize(ackResponse);
            var ackBytes = System.Text.Encoding.UTF8.GetBytes(ackJson);
            await pipe.WriteAsync(ackBytes, ct);
            await pipe.FlushAsync(ct);
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            // Log error if needed
        }
        finally
        {
            try { pipe.Dispose(); } catch { }
        }
    }
}
