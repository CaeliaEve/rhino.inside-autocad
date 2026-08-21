using System;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.IPC;
using Rhino.Inside.AutoCAD.Interop;
using Rhino.Inside.AutoCAD.Interop.Process;
using Rhino.Inside.AutoCAD.Services;
using Rhino.Inside.AutoCAD.UI.Resources.Models;

[assembly: CommandClass(typeof(RhinoInsideAutoCadCommands))]

namespace Rhino.Inside.AutoCAD.Applications;

/// <summary>
/// The commands class for Rhino.Inside.AutoCAD application commands.
/// Orchestrates zero-restart seamless hot switching between Rhino 7 and Rhino 8 workers.
/// </summary>
public class RhinoInsideAutoCadCommands
{
    private static bool _isLaunching;

    private const string _rhinoPreviewButtonId = ApplicationConstants.RhinoPreviewButtonId;
    private const string _grasshopperSolverButtonId = ApplicationConstants.GrasshopperSolverButtonId;
    private const string _rhinocerosPreviewShadedIcon = ApplicationConstants.RhinocerosPreviewShadedIcon;
    private const string _rhinocerosPreviewOffIcon = ApplicationConstants.RhinocerosPreviewOffIcon;
    private const string _grasshopperSolverOnIcon = ApplicationConstants.GrasshopperSolverOnIcon;
    private const string _grasshopperSolverOffIcon = ApplicationConstants.GrasshopperSolverOffIcon;
    private const string _expiredMessage = ApplicationConstants.ExpiredMessage;
    private const string _downloadUrl = ApplicationConstants.DownloadUrl;

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO", CommandFlags.Modal)]
    public static void RHINO()
    {
        if (_isLaunching) return;

        var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;
        var acadHwnd = Autodesk.AutoCAD.ApplicationServices.Core.Application.MainWindow.Handle;

        int version = WorkerProcessManager.Instance.ActiveMajorVersion;
        if (version == 0)
        {
            var locator = new RhinoInstallationLocator();
            var settingsStore = UserSettingsStore.Instance;
            var dialogManager = new RhinoVersionDialogManager();
            var selection = new RhinoVersionSelection(locator, settingsStore, dialogManager);
            var selected = selection.Resolve(out _);
            if (selected == null) return;
            version = selected.MajorVersion;
        }

        _isLaunching = true;
        editor?.WriteMessage(string.Format("\n[Rhino.Inside] Launching Rhino {0}...\n", version));
        _ = Task.Run(async () =>
        {
            try
            {
                var success = await WorkerProcessManager.Instance.LaunchRhinoVersionAsync(version, acadHwnd);
                if (success)
                {
                    editor?.WriteMessage(string.Format("\n[Rhino.Inside] Rhino {0} ready.\n", version));
                }
                else
                {
                    editor?.WriteMessage(string.Format("\n[Rhino.Inside] Failed to launch Rhino {0}.\n", version));
                }
            }
            finally
            {
                _isLaunching = false;
            }
        });
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO7", CommandFlags.Modal)]
    public static void RHINO7()
    {
        if (_isLaunching) return;

        var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;
        var acadHwnd = Autodesk.AutoCAD.ApplicationServices.Core.Application.MainWindow.Handle;

        _isLaunching = true;
        editor?.WriteMessage("\n[Rhino.Inside] Activating Rhino 7...\n");
        _ = Task.Run(async () =>
        {
            try
            {
                var success = await WorkerProcessManager.Instance.LaunchRhinoVersionAsync(7, acadHwnd);
                if (success)
                {
                    editor?.WriteMessage("\n[Rhino.Inside] Rhino 7 activated successfully.\n");
                }
                else
                {
                    editor?.WriteMessage("\n[Rhino.Inside] Failed to activate Rhino 7.\n");
                }
            }
            finally
            {
                _isLaunching = false;
            }
        });
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO8", CommandFlags.Modal)]
    public static void RHINO8()
    {
        if (_isLaunching) return;

        var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;
        var acadHwnd = Autodesk.AutoCAD.ApplicationServices.Core.Application.MainWindow.Handle;

        _isLaunching = true;
        editor?.WriteMessage("\n[Rhino.Inside] Activating Rhino 8...\n");
        _ = Task.Run(async () =>
        {
            try
            {
                var success = await WorkerProcessManager.Instance.LaunchRhinoVersionAsync(8, acadHwnd);
                if (success)
                {
                    editor?.WriteMessage("\n[Rhino.Inside] Rhino 8 activated successfully.\n");
                }
                else
                {
                    editor?.WriteMessage("\n[Rhino.Inside] Failed to activate Rhino 8.\n");
                }
            }
            finally
            {
                _isLaunching = false;
            }
        });
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER", CommandFlags.Modal)]
    public static void GRASSHOPPER()
    {
        if (_isLaunching) return;

        var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;
        var acadHwnd = Autodesk.AutoCAD.ApplicationServices.Core.Application.MainWindow.Handle;

        int version = WorkerProcessManager.Instance.ActiveMajorVersion;
        if (version == 0)
        {
            var locator = new RhinoInstallationLocator();
            var settingsStore = UserSettingsStore.Instance;
            var dialogManager = new RhinoVersionDialogManager();
            var selection = new RhinoVersionSelection(locator, settingsStore, dialogManager);
            var selected = selection.Resolve(out _);
            if (selected == null) return;
            version = selected.MajorVersion;
        }

        _isLaunching = true;
        editor?.WriteMessage(string.Format("\n[Rhino.Inside] Launching Grasshopper {0}...\n", version));
        _ = Task.Run(async () =>
        {
            try
            {
                var success = await WorkerProcessManager.Instance.LaunchGrasshopperVersionAsync(version, acadHwnd);
                if (success)
                {
                    editor?.WriteMessage(string.Format("\n[Rhino.Inside] Grasshopper {0} ready.\n", version));
                }
            }
            finally
            {
                _isLaunching = false;
            }
        });
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GH7", CommandFlags.Modal)]
    public static void GH7()
    {
        if (_isLaunching) return;

        var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;
        var acadHwnd = Autodesk.AutoCAD.ApplicationServices.Core.Application.MainWindow.Handle;

        _isLaunching = true;
        editor?.WriteMessage("\n[Rhino.Inside] Activating Grasshopper 7...\n");
        _ = Task.Run(async () =>
        {
            try
            {
                var success = await WorkerProcessManager.Instance.LaunchGrasshopperVersionAsync(7, acadHwnd);
                if (success)
                {
                    editor?.WriteMessage("\n[Rhino.Inside] Grasshopper 7 activated.\n");
                }
            }
            finally
            {
                _isLaunching = false;
            }
        });
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GH8", CommandFlags.Modal)]
    public static void GH8()
    {
        if (_isLaunching) return;

        var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;
        var acadHwnd = Autodesk.AutoCAD.ApplicationServices.Core.Application.MainWindow.Handle;

        _isLaunching = true;
        editor?.WriteMessage("\n[Rhino.Inside] Activating Grasshopper 8...\n");
        _ = Task.Run(async () =>
        {
            try
            {
                var success = await WorkerProcessManager.Instance.LaunchGrasshopperVersionAsync(8, acadHwnd);
                if (success)
                {
                    editor?.WriteMessage("\n[Rhino.Inside] Grasshopper 8 activated.\n");
                }
            }
            finally
            {
                _isLaunching = false;
            }
        });
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "SWITCH_RHINO_VERSION", CommandFlags.Modal)]
    public static void SWITCH_RHINO_VERSION()
    {
        var acadHwnd = Autodesk.AutoCAD.ApplicationServices.Core.Application.MainWindow.Handle;
        var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;

        int currentVer = WorkerProcessManager.Instance.ActiveMajorVersion;
        int targetVer = (currentVer == 7) ? 8 : 7;

        editor?.WriteMessage(string.Format("\n[Rhino.Inside] Hot-switching active worker from Rhino {0} to Rhino {1}...\n", currentVer, targetVer));
        _ = Task.Run(async () =>
        {
            var success = await WorkerProcessManager.Instance.LaunchRhinoVersionAsync(targetVer, acadHwnd);
            if (success)
            {
                editor?.WriteMessage(string.Format("\n[Rhino.Inside] Successfully hot-switched to Rhino {0}.\n", targetVer));
            }
        });
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "TOGGLE_RHINO_PREVIEW", CommandFlags.Modal)]
    public static void TOGGLE_RHINO_PREVIEW()
    {
        _ = WorkerProcessManager.Instance.SendCommandAsync(IpcCommandType.ToggleRhinoPreview);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER_RECOMPUTE", CommandFlags.Modal)]
    public static void GRASSHOPPER_RECOMPUTE()
    {
        _ = WorkerProcessManager.Instance.SendCommandAsync(IpcCommandType.RecomputeSolution);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER_TOGGLE_SOLVER", CommandFlags.Modal)]
    public static void GRASSHOPPER_TOGGLE_SOLVER()
    {
        _ = WorkerProcessManager.Instance.SendCommandAsync(IpcCommandType.ToggleSolver);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "OPEN_RHINO_VIEWPORT", CommandFlags.Modal)]
    public static void OPEN_RHINO_VIEWPORT()
    {
        _ = WorkerProcessManager.Instance.SendCommandAsync(IpcCommandType.OpenViewport);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO_INSIDE_ABOUT", CommandFlags.Modal)]
    public static void RHINO_INSIDE_ABOUT()
    {
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.3.1";
        Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowAlertDialog(string.Format("Rhino.Inside AutoCAD {0}\nActive Rhino Version: {1}", version, WorkerProcessManager.Instance.ActiveMajorVersion));
    }
}
