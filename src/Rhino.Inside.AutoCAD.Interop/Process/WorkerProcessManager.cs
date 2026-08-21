using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Rhino.Inside.AutoCAD.Core.IPC;
using Rhino.Inside.AutoCAD.Interop.IPC;
using Rhino.Inside.AutoCAD.Services;

namespace Rhino.Inside.AutoCAD.Interop.Process;

/// <summary>
/// Manages the lifecycle of Out-of-Process Rhino Workers (Rhino 7, Rhino 8) and orchestrates zero-restart hot switching.
/// </summary>
public class WorkerProcessManager : IDisposable
{
    private static readonly Lazy<WorkerProcessManager> _instance = new(() => new WorkerProcessManager());
    public static WorkerProcessManager Instance => _instance.Value;

    private readonly WindowsJobObject _jobObject = new();
    private readonly ConcurrentDictionary<int, WorkerSession> _sessions = new();
    private int _activeMajorVersion = 0;
    private readonly SemaphoreSlim _switchLock = new(1, 1);
    private bool _isDisposed;

    public int ActiveMajorVersion => _activeMajorVersion;
    public bool HasActiveWorker => _activeMajorVersion > 0 && _sessions.TryGetValue(_activeMajorVersion, out var s) && s.IsAlive;

    public event Action<int>? VersionSwitched;
    public event Action<int, string>? WorkerCrashed;

    private WorkerProcessManager()
    {
    }

    private class WorkerSession : IDisposable
    {
        public int MajorVersion { get; set; }
        public System.Diagnostics.Process? Process { get; set; }
        public NamedPipeClientTransport? Transport { get; set; }
        public SharedMemoryBuffer? SharedMemory { get; set; }
        public string PipeName { get; set; } = string.Empty;
        public string MapName { get; set; } = string.Empty;

        public bool IsAlive => this.Process != null && !this.Process.HasExited && (this.Transport?.IsConnected ?? false);

        public void Dispose()
        {
            this.Transport?.Dispose();
            this.SharedMemory?.Dispose();
            try
            {
                if (this.Process != null && !this.Process.HasExited)
                {
                    this.Process.Kill();
                }
            }
            catch { }
            this.Process?.Dispose();
        }
    }

    /// <summary>
    /// Ensures that a worker session for the specified Rhino major version (e.g. 7 or 8) is running and active.
    /// </summary>
    public async Task<bool> ActivateVersionAsync(int majorVersion, IntPtr hostWindowHandle = default, CancellationToken cancellationToken = default)
    {
        await _switchLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_activeMajorVersion == majorVersion && _sessions.TryGetValue(majorVersion, out var current) && current.IsAlive)
            {
                return true;
            }

            // Suspend previous active session if any
            if (_activeMajorVersion > 0 && _activeMajorVersion != majorVersion && _sessions.TryGetValue(_activeMajorVersion, out var previous) && previous.IsAlive)
            {
                _ = previous.Transport?.SendMessageAsync(IpcMessage.Create(IpcCommandType.Suspend));
            }

            // Ensure target worker session is started
            var session = await this.GetOrCreateSessionAsync(majorVersion, hostWindowHandle, cancellationToken).ConfigureAwait(false);
            if (session == null || !session.IsAlive)
            {
                return false;
            }

            // Resume target worker
            await session.Transport!.SendMessageAsync(IpcMessage.Create(IpcCommandType.Resume)).ConfigureAwait(false);

            _activeMajorVersion = majorVersion;
            this.VersionSwitched?.Invoke(majorVersion);

            LoggerService.Instance.LogMessage($"[WorkerProcessManager] Successfully activated Rhino {majorVersion} Worker session.");
            return true;
        }
        finally
        {
            _switchLock.Release();
        }
    }

    private async Task<WorkerSession?> GetOrCreateSessionAsync(int majorVersion, IntPtr hostWindowHandle, CancellationToken cancellationToken)
    {
        if (_sessions.TryGetValue(majorVersion, out var existing) && existing.IsAlive)
        {
            return existing;
        }

        existing?.Dispose();
        _sessions.TryRemove(majorVersion, out _);

        int hostPid = System.Diagnostics.Process.GetCurrentProcess().Id;
        string pipeName = $"RhinoInside_AutoCAD_Pipe_{hostPid}_R{majorVersion}";
        string mapName = $"RhinoInside_AutoCAD_MMF_{hostPid}_R{majorVersion}";

        var workerExePath = this.ResolveWorkerExecutablePath();
        if (!File.Exists(workerExePath))
        {
            LoggerService.Instance.LogError(new FileNotFoundException($"Worker executable not found at: {workerExePath}"));
            return null;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = workerExePath,
            Arguments = $"--version {majorVersion} --pipe {pipeName} --map {mapName} --hostpid {hostPid} --hwnd {hostWindowHandle.ToInt64()}",
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        var process = System.Diagnostics.Process.Start(startInfo);
        if (process == null)
        {
            LoggerService.Instance.LogError(new InvalidOperationException($"Failed to spawn Worker process for Rhino {majorVersion}."));
            return null;
        }

        _jobObject.AssignProcess(process);

        var transport = new NamedPipeClientTransport(pipeName);
        var connected = false;

        // Connect with retry loop
        for (int i = 0; i < 20; i++)
        {
            if (process.HasExited) break;
            if (await transport.ConnectAsync(1000, cancellationToken).ConfigureAwait(false))
            {
                connected = true;
                break;
            }
            await Task.Delay(300, cancellationToken).ConfigureAwait(false);
        }

        if (!connected || process.HasExited)
        {
            process.Dispose();
            transport.Dispose();
            LoggerService.Instance.LogError(new TimeoutException($"Timed out connecting to Rhino {majorVersion} Worker pipe: {pipeName}"));
            return null;
        }

        var sharedMemory = SharedMemoryBuffer.Create(mapName);

        var session = new WorkerSession
        {
            MajorVersion = majorVersion,
            Process = process,
            Transport = transport,
            SharedMemory = sharedMemory,
            PipeName = pipeName,
            MapName = mapName
        };

        transport.Disconnected += () =>
        {
            LoggerService.Instance.LogMessage($"[WorkerProcessManager] Rhino {majorVersion} Worker disconnected.");
            this.WorkerCrashed?.Invoke(majorVersion, $"Rhino {majorVersion} Worker connection lost.");
        };

        _sessions[majorVersion] = session;
        return session;
    }

    private string ResolveWorkerExecutablePath()
    {
        var asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        var directPath = Path.Combine(asmDir, "Rhino.Inside.AutoCAD.Worker.exe");
        if (File.Exists(directPath)) return directPath;

        // Fallback to parent or runtime dirs
        var bundleDir = Path.GetFullPath(Path.Combine(asmDir, "..", "..", ".."));
        var candidate = Path.Combine(bundleDir, "Worker", "Rhino.Inside.AutoCAD.Worker.exe");
        if (File.Exists(candidate)) return candidate;

        return directPath;
    }

    public async Task<IpcMessage?> SendCommandAsync(IpcCommandType command, string? payload = null, int timeoutMs = 5000)
    {
        if (_activeMajorVersion == 0 || !_sessions.TryGetValue(_activeMajorVersion, out var session) || !session.IsAlive)
        {
            return IpcMessage.Fail(string.Empty, "No active Rhino worker session.");
        }

        var request = IpcMessage.Create(command, payload);
        return await session.Transport!.SendRequestAsync(request, timeoutMs).ConfigureAwait(false);
    }

    /// <summary>
    /// Launches Rhino window for the specified major version (7 or 8), activating the worker first.
    /// </summary>
    public async Task<bool> LaunchRhinoVersionAsync(int majorVersion, IntPtr hostWindowHandle = default)
    {
        var activated = await this.ActivateVersionAsync(majorVersion, hostWindowHandle).ConfigureAwait(false);
        if (!activated) return false;

        var response = await this.SendCommandAsync(IpcCommandType.LaunchRhino).ConfigureAwait(false);
        return response?.Success ?? false;
    }

    /// <summary>
    /// Launches Grasshopper canvas for the specified major version (7 or 8), activating the worker first.
    /// </summary>
    public async Task<bool> LaunchGrasshopperVersionAsync(int majorVersion, IntPtr hostWindowHandle = default)
    {
        var activated = await this.ActivateVersionAsync(majorVersion, hostWindowHandle).ConfigureAwait(false);
        if (!activated) return false;

        var response = await this.SendCommandAsync(IpcCommandType.LaunchGrasshopper).ConfigureAwait(false);
        return response?.Success ?? false;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        foreach (var s in _sessions.Values)
        {
            s.Dispose();
        }
        _sessions.Clear();
        _jobObject.Dispose();
        _switchLock.Dispose();
    }
}
