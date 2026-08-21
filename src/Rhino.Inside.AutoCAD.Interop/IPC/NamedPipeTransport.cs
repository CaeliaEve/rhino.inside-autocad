using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rhino.Inside.AutoCAD.Core.IPC;

namespace Rhino.Inside.AutoCAD.Interop.IPC;

/// <summary>
/// High-throughput asynchronous Named Pipe client for bidirectional RPC.
/// </summary>
public class NamedPipeClientTransport : IDisposable
{
    private readonly string _pipeName;
    private NamedPipeClientStream? _pipeClient;
    private StreamWriter? _writer;
    private StreamReader? _reader;
    private readonly CancellationTokenSource _cts = new();
    private bool _isDisposed;

    public event Action<IpcMessage>? MessageReceived;
    public event Action? Disconnected;

    public bool IsConnected => _pipeClient?.IsConnected ?? false;

    public NamedPipeClientTransport(string pipeName)
    {
        _pipeName = pipeName;
    }

    public async Task<bool> ConnectAsync(int timeoutMs = 5000, CancellationToken cancellationToken = default)
    {
        try
        {
            _pipeClient?.Dispose();
            _pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
            await _pipeClient.ConnectAsync(timeoutMs, linkedCts.Token).ConfigureAwait(false);

            _writer = new StreamWriter(_pipeClient, Encoding.UTF8, 4096, leaveOpen: true) { AutoFlush = true };
            _reader = new StreamReader(_pipeClient, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, 4096, leaveOpen: true);

            _ = Task.Run(this.ListenLoopAsync, _cts.Token);
            return true;
        }
        catch
        {
            _pipeClient?.Dispose();
            _pipeClient = null;
            return false;
        }
    }

    public async Task<bool> SendMessageAsync(IpcMessage message)
    {
        if (_writer == null || !this.IsConnected) return false;
        try
        {
            var json = message.ToJson();
            await _writer.WriteLineAsync(json).ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IpcMessage?> SendRequestAsync(IpcMessage request, int timeoutMs = 5000)
    {
        if (_writer == null || !this.IsConnected) return null;

        var tcs = new TaskCompletionSource<IpcMessage>();
        void Handler(IpcMessage msg)
        {
            if (msg.Id == request.Id)
            {
                tcs.TrySetResult(msg);
            }
        }

        this.MessageReceived += Handler;
        try
        {
            var sent = await this.SendMessageAsync(request).ConfigureAwait(false);
            if (!sent) return null;

            using var timeoutCts = new CancellationTokenSource(timeoutMs);
            using (timeoutCts.Token.Register(() => tcs.TrySetCanceled()))
            {
                return await tcs.Task.ConfigureAwait(false);
            }
        }
        catch
        {
            return null;
        }
        finally
        {
            this.MessageReceived -= Handler;
        }
    }

    private async Task ListenLoopAsync()
    {
        var buffer = new StringBuilder();
        try
        {
            while (!_cts.Token.IsCancellationRequested && _reader != null && this.IsConnected)
            {
                var line = await _reader.ReadLineAsync().ConfigureAwait(false);
                if (line == null) break;

                var message = IpcMessage.FromJson(line);
                if (message != null)
                {
                    this.MessageReceived?.Invoke(message);
                }
            }
        }
        catch
        {
            // Pipe broken or disconnected
        }
        finally
        {
            this.Disconnected?.Invoke();
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _cts.Cancel();
        _writer?.Dispose();
        _reader?.Dispose();
        _pipeClient?.Dispose();
        _cts.Dispose();
    }
}

/// <summary>
/// Asynchronous Named Pipe Server hosted by a Rhino Worker process.
/// </summary>
public class NamedPipeServerTransport : IDisposable
{
    private readonly string _pipeName;
    private NamedPipeServerStream? _pipeServer;
    private StreamWriter? _writer;
    private StreamReader? _reader;
    private readonly CancellationTokenSource _cts = new();
    private bool _isDisposed;

    public event Action<IpcMessage>? MessageReceived;
    public event Action? ClientConnected;
    public event Action? ClientDisconnected;

    public bool IsConnected => _pipeServer?.IsConnected ?? false;

    public NamedPipeServerTransport(string pipeName)
    {
        _pipeName = pipeName;
    }

    public void Start()
    {
        _ = Task.Run(this.ServerLoopAsync, _cts.Token);
    }

    private async Task ServerLoopAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                _pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await _pipeServer.WaitForConnectionAsync(_cts.Token).ConfigureAwait(false);

                _writer = new StreamWriter(_pipeServer, Encoding.UTF8, 4096, leaveOpen: true) { AutoFlush = true };
                _reader = new StreamReader(_pipeServer, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, 4096, leaveOpen: true);

                this.ClientConnected?.Invoke();

                while (!_cts.Token.IsCancellationRequested && this.IsConnected && _reader != null)
                {
                    var line = await _reader.ReadLineAsync().ConfigureAwait(false);
                    if (line == null) break;

                    var message = IpcMessage.FromJson(line);
                    if (message != null)
                    {
                        this.MessageReceived?.Invoke(message);
                    }
                }
            }
            catch
            {
                // Client disconnected
            }
            finally
            {
                this.ClientDisconnected?.Invoke();
                _writer?.Dispose();
                _reader?.Dispose();
                _pipeServer?.Dispose();
            }
        }
    }

    public async Task<bool> SendMessageAsync(IpcMessage message)
    {
        if (_writer == null || !this.IsConnected) return false;
        try
        {
            var json = message.ToJson();
            await _writer.WriteLineAsync(json).ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _cts.Cancel();
        _writer?.Dispose();
        _reader?.Dispose();
        _pipeServer?.Dispose();
        _cts.Dispose();
    }
}
