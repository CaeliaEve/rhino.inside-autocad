using System;
using System.Threading.Tasks;

namespace Rhino.Inside.AutoCAD.Core.IPC;

/// <summary>
/// High-level client for Grasshopper 8 to communicate with AutoCAD over the Live Link IPC bridge.
/// </summary>
public class LiveLinkClient : IDisposable
{
    private static readonly Lazy<LiveLinkClient> _instance = new(() => new LiveLinkClient());
    public static LiveLinkClient Instance => _instance.Value;

    private readonly NamedPipeClient _client = new();
    private bool _isDisposed;

    public bool IsConnected => _client.IsConnected;

    public event Action? Connected;
    public event Action? Disconnected;
    public event Action<IpcMessage>? MessageReceived;

    private LiveLinkClient()
    {
        _client.Connected += () => Connected?.Invoke();
        _client.Disconnected += () => Disconnected?.Invoke();
        _client.MessageReceived += msg => MessageReceived?.Invoke(msg);
    }

    public async Task<bool> EnsureConnectedAsync(int timeoutMs = 1000)
    {
        if (_client.IsConnected) return true;
        return await _client.ConnectAsync(timeoutMs).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a Bake payload to AutoCAD.
    /// </summary>
    public async Task<bool> SendBakeAsync(BakePayload payload)
    {
        if (!await EnsureConnectedAsync().ConfigureAwait(false)) return false;

        var message = IpcMessage.Create(IpcCommandType.BakeRequest, payload);
        return await _client.SendMessageAsync(message).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a Transient Preview payload to AutoCAD.
    /// </summary>
    public async Task<bool> SendPreviewAsync(byte[] geometry3dmBytes)
    {
        if (!await EnsureConnectedAsync().ConfigureAwait(false)) return false;

        var message = new IpcMessage
        {
            CommandType = IpcCommandType.TransientPreview,
            Payload = geometry3dmBytes
        };
        return await _client.SendMessageAsync(message).ConfigureAwait(false);
    }

    /// <summary>
    /// Clears any active Transient Preview in AutoCAD.
    /// </summary>
    public async Task<bool> ClearPreviewAsync()
    {
        if (!_client.IsConnected) return false;

        var message = new IpcMessage
        {
            CommandType = IpcCommandType.ClearPreview,
            Payload = Array.Empty<byte>()
        };
        return await _client.SendMessageAsync(message).ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _client.Dispose();
    }
}
