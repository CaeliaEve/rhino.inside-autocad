using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Rhino;
using Rhino.Inside.AutoCAD.Core.IPC;
using Rhino.Inside.AutoCAD.Interop;
using Rhino.Inside.AutoCAD.Interop.IPC;
using Rhino.Inside.AutoCAD.Services;

namespace Rhino.Inside.AutoCAD.Worker;

internal static class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private const int GWLP_HWNDPARENT = -8;
    private const int SW_SHOW = 5;

    private static NamedPipeServerTransport? _serverTransport;
    private static SharedMemoryBuffer? _sharedMemory;
    private static IntPtr _hostHwnd = IntPtr.Zero;
    private static int _targetMajorVersion = 8;
    private static bool _isSuspended = false;

    [STAThread]
    private static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        string pipeName = string.Empty;
        string mapName = string.Empty;
        int hostPid = 0;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--version" && i + 1 < args.Length && int.TryParse(args[i + 1], out var v))
            {
                _targetMajorVersion = v;
            }
            else if (args[i] == "--pipe" && i + 1 < args.Length)
            {
                pipeName = args[i + 1];
            }
            else if (args[i] == "--map" && i + 1 < args.Length)
            {
                mapName = args[i + 1];
            }
            else if (args[i] == "--hostpid" && i + 1 < args.Length && int.TryParse(args[i + 1], out var pid))
            {
                hostPid = pid;
            }
            else if (args[i] == "--hwnd" && i + 1 < args.Length && long.TryParse(args[i + 1], out var hwnd))
            {
                _hostHwnd = new IntPtr(hwnd);
            }
        }

        if (string.IsNullOrEmpty(pipeName))
        {
            pipeName = $"RhinoInside_AutoCAD_Worker_R{_targetMajorVersion}";
        }

        // Host Watchdog: Terminate if AutoCAD host process dies
        if (hostPid > 0)
        {
            var hostWatcher = new Thread(() =>
            {
                try
                {
                    var hostProcess = System.Diagnostics.Process.GetProcessById(hostPid);
                    hostProcess.WaitForExit();
                }
                catch { }
                finally
                {
                    Environment.Exit(0);
                }
            })
            { IsBackground = true };
            hostWatcher.Start();
        }

        // Locate Rhino Installation for requested version
        var locator = new RhinoInstallationLocator();
        var installations = locator.Locate();
        var selected = installations.FirstOrDefault(x => x.MajorVersion == _targetMajorVersion)
                       ?? installations.FirstOrDefault();

        if (selected == null)
        {
            Environment.Exit(1);
            return;
        }

        // Bind assembly resolution to selected Rhino installation
        RhinoCoreExtension.HostWindowHandle = _hostHwnd;
        RhinoCoreExtension.BindTo(selected);

        // Initialize IPC channels
        _serverTransport = new NamedPipeServerTransport(pipeName);
        _serverTransport.MessageReceived += OnIpcMessageReceived;
        _serverTransport.Start();

        if (!string.IsNullOrEmpty(mapName))
        {
            try
            {
                _sharedMemory = SharedMemoryBuffer.Open(mapName);
            }
            catch
            {
                _sharedMemory = SharedMemoryBuffer.Create(mapName);
            }
        }

        // Start message pump
        Application.Run(new WorkerApplicationContext());
    }

    private class WorkerApplicationContext : ApplicationContext
    {
        public WorkerApplicationContext()
        {
            // Keep process running in background
        }
    }

    private static void OnIpcMessageReceived(IpcMessage msg)
    {
        try
        {
            switch (msg.Command)
            {
                case IpcCommandType.Ping:
                    _serverTransport?.SendMessageAsync(IpcMessage.Ok(msg.Id, "Pong"));
                    break;

                case IpcCommandType.GetStatus:
                    _serverTransport?.SendMessageAsync(IpcMessage.Ok(msg.Id, $"Rhino {_targetMajorVersion} Active={!_isSuspended}"));
                    break;

                case IpcCommandType.LaunchRhino:
                    RhinoCoreExtension.Instance.ValidateRhinoCore();
                    var hwnd = RhinoApp.MainWindowHandle();
                    if (hwnd != IntPtr.Zero)
                    {
                        ShowWindow(hwnd, SW_SHOW);
                        SetForegroundWindow(hwnd);
                    }
                    _serverTransport?.SendMessageAsync(IpcMessage.Ok(msg.Id));
                    break;

                case IpcCommandType.LaunchGrasshopper:
                    RhinoCoreExtension.Instance.ValidateRhinoCore();
                    RhinoApp.RunScript("!_-Grasshopper _Window _Show _Enter", false);
                    _serverTransport?.SendMessageAsync(IpcMessage.Ok(msg.Id));
                    break;

                case IpcCommandType.OpenViewport:
                    RhinoCoreExtension.Instance.ValidateRhinoCore();
                    RhinoApp.RunScript("!_NewFloatingViewport", false);
                    _serverTransport?.SendMessageAsync(IpcMessage.Ok(msg.Id));
                    break;

                case IpcCommandType.RecomputeSolution:
                    RhinoApp.RunScript("!_-Grasshopper _Solver _Recompute _Enter", false);
                    _serverTransport?.SendMessageAsync(IpcMessage.Ok(msg.Id));
                    break;

                case IpcCommandType.ToggleSolver:
                    RhinoApp.RunScript("!_-Grasshopper _Solver _Toggle _Enter", false);
                    _serverTransport?.SendMessageAsync(IpcMessage.Ok(msg.Id));
                    break;

                case IpcCommandType.Suspend:
                    _isSuspended = true;
                    // Suspend GH computation and release working set
                    RhinoApp.RunScript("!_-Grasshopper _Solver _Disable _Enter", false);
                    _serverTransport?.SendMessageAsync(IpcMessage.Ok(msg.Id, "Suspended"));
                    break;

                case IpcCommandType.Resume:
                    _isSuspended = false;
                    RhinoApp.RunScript("!_-Grasshopper _Solver _Enable _Enter", false);
                    _serverTransport?.SendMessageAsync(IpcMessage.Ok(msg.Id, "Resumed"));
                    break;

                case IpcCommandType.Shutdown:
                    _serverTransport?.SendMessageAsync(IpcMessage.Ok(msg.Id, "Shutting down"));
                    Environment.Exit(0);
                    break;

                default:
                    _serverTransport?.SendMessageAsync(IpcMessage.Fail(msg.Id, $"Unknown command: {msg.Command}"));
                    break;
            }
        }
        catch (Exception ex)
        {
            _serverTransport?.SendMessageAsync(IpcMessage.Fail(msg.Id, ex.Message));
        }
    }
}
