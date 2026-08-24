using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Rhino.Inside.AutoCAD.Core.IPC;

/// <summary>
/// High-performance asynchronous Named Pipe client used inside Rhino 8 / Grasshopper.
/// </summary>
public class NamedPipeClient : IDisposable
{
    private NamedPipeClientStream? _pipeClient;
    private CancellationTokenSource? _cts;
    private Task? _readTask;
    private bool _isDisposed;
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    public bool IsConnected => _pipeClient != null && _pipeClient.IsConnected;

    public event Action? Connected;
    public event Action? Disconnected;
    public event Action<IpcMessage>? MessageReceived;

    public async Task<bool> ConnectAsync(int timeoutMs = 2000, CancellationToken cancellationToken = default)
    {
        if (IsConnected) return true;

        try
        {
            _pipeClient?.Dispose();
            _pipeClient = new NamedPipeClientStream(".", NamedPipeServer.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

            await _pipeClient.ConnectAsync(timeoutMs, cancellationToken).ConfigureAwait(false);

            _cts = new CancellationTokenSource();
            _readTask = Task.Run(() => ReadLoopAsync(_cts.Token));

            Connected?.Invoke();
            return true;
        }
        catch
        {
            _pipeClient?.Dispose();
            _pipeClient = null;
            return false;
        }
    }

    private async Task ReadLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (_pipeClient != null && _pipeClient.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                var msg = await IpcMessage.ReadMessageAsync(_pipeClient, cancellationToken).ConfigureAwait(false);
                if (msg == null) break;
                MessageReceived?.Invoke(msg);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NamedPipeClient] ReadLoop exception: {ex.Message}");
        }
        finally
        {
            Disconnected?.Invoke();
            try { _pipeClient?.Dispose(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[NamedPipeClient] Dispose error: {ex.Message}"); }
            _pipeClient = null;
        }
    }

    public async Task<bool> SendMessageAsync(IpcMessage message, CancellationToken cancellationToken = default)
    {
        if (!IsConnected || _pipeClient == null) return false;

        await _sendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!IsConnected || _pipeClient == null) return false;
            await IpcMessage.WriteMessageAsync(_pipeClient, message, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NamedPipeClient] SendMessage error: {ex.Message}");
            return false;
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public void Disconnect()
    {
        try
        {
            _cts?.Cancel();
            _pipeClient?.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NamedPipeClient] Disconnect error: {ex.Message}");
        }
        _pipeClient = null;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        Disconnect();
        _cts?.Dispose();
        _sendLock.Dispose();
    }
}
