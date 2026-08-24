using System;
using System.Threading.Tasks;

namespace Rhino.Inside.AutoCAD.Core.IPC;

/// <summary>
/// Central manager for the AutoCAD - Rhino 8 Live Link subsystem.
/// Controls the Live Link ON/OFF state, Pipe Server lifecycle, and dispatches IPC messages.
/// </summary>
public class LiveLinkManager : IDisposable
{
    private static readonly Lazy<LiveLinkManager> _instance = new(() => new LiveLinkManager());
    public static LiveLinkManager Instance => _instance.Value;

    private readonly NamedPipeServer _server = new();
    private bool _isEnabled = true; // Enabled by default for seamless connection
    private bool _isDisposed;

    public bool IsEnabled => _isEnabled;
    public bool IsClientConnected => _server.IsConnected;

    public event Action<bool>? LiveLinkStateChanged;
    public event Action? ClientConnected;
    public event Action? ClientDisconnected;
    public event Action<IpcMessage>? MessageReceived;

    private LiveLinkManager()
    {
        _server.ClientConnected += () => ClientConnected?.Invoke();
        _server.ClientDisconnected += () => ClientDisconnected?.Invoke();
        _server.MessageReceived += msg => MessageReceived?.Invoke(msg);

        // Start listening on initialization
        _server.Start();
    }

    /// <summary>
    /// Toggles the Live Link state between ON and OFF.
    /// </summary>
    public bool ToggleLiveLink()
    {
        SetLiveLinkEnabled(!_isEnabled);
        return _isEnabled;
    }

    /// <summary>
    /// Explicitly sets the Live Link enabled state.
    /// </summary>
    public void SetLiveLinkEnabled(bool enabled)
    {
        if (_isEnabled == enabled) return;

        _isEnabled = enabled;

        if (_isEnabled)
        {
            _server.Start();
        }
        else
        {
            _server.Stop();
        }

        LiveLinkStateChanged?.Invoke(_isEnabled);
    }

    /// <summary>
    /// Asynchronously broadcasts an IPC message to the connected Rhino 8 client.
    /// </summary>
    public async Task SendMessageAsync(IpcMessage message)
    {
        if (!_isEnabled) return;
        await _server.SendMessageAsync(message).ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _server.Dispose();
    }
}
