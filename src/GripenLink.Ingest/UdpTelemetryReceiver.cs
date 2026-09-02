using System.Net.Sockets;

namespace GripenLink.Ingest;

/// <summary>
/// Recebe datagramas UDP (telemetria do Export.lua do DCS) e dispara
/// <see cref="DatagramReceived"/> para cada mensagem.
/// </summary>
public sealed class UdpTelemetryReceiver : IDisposable
{
    private readonly UdpClient _client;
    private readonly CancellationTokenSource _cts = new();

    public event Action<byte[]>? DatagramReceived;

    public UdpTelemetryReceiver(int port)
    {
        _client = new UdpClient(port);
    }

    public void Start() => _ = Task.Run(ReceiveLoopAsync);

    private async Task ReceiveLoopAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var result = await _client.ReceiveAsync(_cts.Token);
                DatagramReceived?.Invoke(result.Buffer);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _client.Dispose();
        _cts.Dispose();
    }
}
