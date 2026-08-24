using System;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Rhino.Inside.AutoCAD.Core.IPC;

/// <summary>
/// High-performance asynchronous Named Pipe server hosted inside AutoCAD.
/// </summary>
public class NamedPipeServer : IDisposable
{
    public const string PipeName = "RhinoInside_AutoCAD_Bridge_v8";
    
    private NamedPipeServerStream? _pipeServer;
    private CancellationTokenSource? _cts;
    private Task? _listenTask;
    private bool _isDisposed;

    public bool IsConnected => _pipeServer != null && _pipeServer.IsConnected;

    public event Action? ClientConnected;
    public event Action? ClientDisconnected;
    public event Action<IpcMessage>? MessageReceived;

    public void Start()
    {
        if (_listenTask != null && !_listenTask.IsCompleted) return;

        _cts = new CancellationTokenSource();
        _listenTask = Task.Run(() => ServerLoopAsync(_cts.Token));
    }

    private async Task ServerLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _pipeServer = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await _pipeServer.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                ClientConnected?.Invoke();

                while (_pipeServer.IsConnected && !cancellationToken.IsCancellationRequested)
                {
                    var msg = await IpcMessage.ReadMessageAsync(_pipeServer, cancellationToken).ConfigureAwait(false);
                    if (msg == null) break; // Client disconnected
                    MessageReceived?.Invoke(msg);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NamedPipeServer] Server loop exception: {ex.Message}");
                await Task.Delay(500, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ClientDisconnected?.Invoke();
                try
                {
                    _pipeServer?.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[NamedPipeServer] Dispose error: {ex.Message}");
                }
                _pipeServer = null;
            }
        }
    }

    public async Task SendMessageAsync(IpcMessage message, CancellationToken cancellationToken = default)
    {
        if (_pipeServer != null && _pipeServer.IsConnected)
        {
            try
            {
                await IpcMessage.WriteMessageAsync(_pipeServer, message, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NamedPipeServer] SendMessage error: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        try
        {
            _cts?.Cancel();
            _pipeServer?.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NamedPipeServer] Stop error: {ex.Message}");
        }
        _pipeServer = null;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        Stop();
        _cts?.Dispose();
    }
}
