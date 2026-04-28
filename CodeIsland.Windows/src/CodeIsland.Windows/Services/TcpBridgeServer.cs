using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using CodeIsland.Windows.Models;

namespace CodeIsland.Windows.Services;

/// <summary>
/// TCP fallback server for WSL bridge compatibility.
/// Listens on 127.0.0.1:29418 for Linux bridge connections.
/// Protocol is identical to the Named Pipe server.
/// </summary>
public sealed class TcpBridgeServer
{
    private const int Port = 29418;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly SessionStore _sessionStore;
    private readonly CancellationTokenSource _cts = new();
    private TcpListener? _listener;
    private readonly List<Task> _handlerTasks = [];
    private bool _running;

    public TcpBridgeServer(SessionStore sessionStore)
    {
        _sessionStore = sessionStore;
    }

    public Task StartAsync()
    {
        if (_running) return Task.CompletedTask;
        _running = true;

        var listenerTask = Task.Run(() => ListenLoop(_cts.Token));
        _handlerTasks.Add(listenerTask);

        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _running = false;
        _cts.Cancel();
        _listener?.Stop();

        try
        {
            await Task.WhenAll(_handlerTasks);
        }
        catch (OperationCanceledException) { }
    }

    private async Task ListenLoop(CancellationToken ct)
    {
        _listener = new TcpListener(IPAddress.Loopback, Port);

        try
        {
            _listener.Start();

            while (!ct.IsCancellationRequested && _running)
            {
                var client = await _listener.AcceptTcpClientAsync(ct);
                _ = Task.Run(() => HandleClient(client, ct), ct);
            }
        }
        catch (OperationCanceledException) { }
        catch (SocketException) when (!_running) { }
    }

    private async Task HandleClient(TcpClient client, CancellationToken ct)
    {
        try
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, leaveOpen: true);

            var json = await reader.ReadLineAsync(ct);
            if (string.IsNullOrEmpty(json)) return;

            var envelope = JsonSerializer.Deserialize<BridgeEnvelope>(json, JsonOptions);
            if (envelope == null) return;

            var hookEvent = HookEvent.FromBridgeEnvelope(envelope);

            if (envelope.ExpectsResponse)
            {
                // For TCP, we need to create a connection-like abstraction
                // that writes back to the TCP stream
                var connection = new TcpConnection(stream, client, envelope.Id);
                _sessionStore.RegisterPendingConnection(
                    envelope.SessionKey,
                    envelope.Metadata.TryGetValue("toolUseId", out var toolUseId) ? toolUseId : string.Empty,
                    envelope.Metadata.TryGetValue("tool", out var tool) ? tool : "unknown",
                    envelope.Preview,
                    connection);
                return;
            }

            _sessionStore.Process(new SessionEvent.HookReceived(hookEvent));

            var ackResponse = new BridgeResponse
            {
                RequestId = envelope.Id
            };
            var ackJson = JsonSerializer.Serialize(ackResponse);
            var ackBytes = System.Text.Encoding.UTF8.GetBytes(ackJson + "\n");
            await stream.WriteAsync(ackBytes, ct);
            await stream.FlushAsync(ct);
        }
        catch (OperationCanceledException) { }
        catch (Exception) { }
        finally
        {
            try { client.Dispose(); } catch { }
        }
    }
}

/// <summary>
/// TCP connection wrapper for sending responses back over TCP.
/// </summary>
public sealed class TcpConnection : IDisposable
{
    public Guid RequestId { get; }
    private readonly NetworkStream _stream;
    private readonly TcpClient _client;

    public TcpConnection(NetworkStream stream, TcpClient client, Guid requestId)
    {
        _stream = stream;
        _client = client;
        RequestId = requestId;
    }

    public void SendResponse(BridgeResponse response)
    {
        try
        {
            var json = JsonSerializer.Serialize(response);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json + "\n");
            _stream.Write(bytes, 0, bytes.Length);
            _stream.Flush();
        }
        catch { }
    }

    public void Dispose()
    {
        try { _client.Dispose(); } catch { }
    }
}
