using Autodesk.AutoCAD.Runtime;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Interop;
using Rhino.Inside.AutoCAD.Interop.Process;
using Rhino.Inside.AutoCAD.Services;
using Rhino.Inside.AutoCAD.UI.Resources.Models;

[assembly: CommandClass(typeof(RhinoInsideAutoCadCommands))]

namespace Rhino.Inside.AutoCAD.Applications;

/// <summary>
/// The commands class for Rhino.Inside.AutoCAD application commands. This class contains
/// all the command methods that can be invoked from AutoCAD to interact with Rhino and Grasshopper.
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
    private const string _grasshopperCommandName = ApplicationConstants.GrasshopperCommandName;
    private const string _packageManagerCommandName = ApplicationConstants.PackageManagerCommandName;
    private const string _grasshopperPlayerCommandName = ApplicationConstants.GrasshopperPlayerCommandName;
    private const string _newFloatingViewportScript = ApplicationConstants.NewFloatingViewportScript;
    private const string _expiredMessage = ApplicationConstants.ExpiredMessage;
    private const string _downloadUrl = ApplicationConstants.DownloadUrl;

    /// <summary>
    /// Checks whether the application is unusable, reporting why if it is.
    /// </summary>
    private static bool CheckApplicationIsUnusable(int? targetVersion = null)
    {
        if (RhinoInsideAutoCadExtension.LoadFailureMessage is { } loadFailureMessage)
        {
            Autodesk.AutoCAD.ApplicationServices.Core.Application
                .ShowAlertDialog(loadFailureMessage);

            return true;
        }

        if (RhinoInsideAutoCadExtension.Application is null)
        {
            if (!RhinoInsideAutoCadExtension.EnsureInitialized(targetVersion))
            {
                return true;
            }
        }

        if (RhinoInsideAutoCadExtension.IsExpired)
        {
            ShowExpirationDialog();

            return true;
        }

        return false;
    }

    /// <summary>
    /// Shows the expiration dialog with download and close buttons.
    /// </summary>
    private static void ShowExpirationDialog()
    {
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
        var manager = new ExpirationDialogManager(_expiredMessage, _downloadUrl, version);
        manager.Show();
    }

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
        editor?.WriteMessage($"\n[Rhino.Inside] Launching Rhino {version}...\n");
        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                await WorkerProcessManager.Instance.LaunchRhinoVersionAsync(version, acadHwnd);
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
        editor?.WriteMessage("\n[Rhino.Inside] Launching Rhino 7...\n");
        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                await WorkerProcessManager.Instance.LaunchRhinoVersionAsync(7, acadHwnd);
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
        editor?.WriteMessage("\n[Rhino.Inside] Launching Rhino 8...\n");
        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                await WorkerProcessManager.Instance.LaunchRhinoVersionAsync(8, acadHwnd);
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
        editor?.WriteMessage($"\n[Rhino.Inside] Launching Grasshopper {version}...\n");
        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                await WorkerProcessManager.Instance.LaunchGrasshopperVersionAsync(version, acadHwnd);
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
        editor?.WriteMessage("\n[Rhino.Inside] Launching Grasshopper 7...\n");
        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                await WorkerProcessManager.Instance.LaunchGrasshopperVersionAsync(7, acadHwnd);
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
        editor?.WriteMessage("\n[Rhino.Inside] Launching Grasshopper 8...\n");
        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                await WorkerProcessManager.Instance.LaunchGrasshopperVersionAsync(8, acadHwnd);
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
        RhinoInsideAutoCadExtension.PromptSwitchVersion();
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "TOGGLE_RHINO_PREVIEW", CommandFlags.Modal)]
    public static void TOGGLE_RHINO_PREVIEW()
    {
        if (CheckApplicationIsUnusable())
            return;

        var application = RhinoInsideAutoCadExtension.Application;
        var rhinoInsideManager = application!.RhinoInsideManager;
        var rhinoObjectPreview = rhinoInsideManager.RhinoPreviewServer;
        rhinoObjectPreview.ToggleVisibility();

        var buttonReplacer = new ButtonIconReplacer(_rhinoPreviewButtonId);
        var imagePath = rhinoObjectPreview.Visible
            ? _rhinocerosPreviewShadedIcon
            : _rhinocerosPreviewOffIcon;
        buttonReplacer.Replace(imagePath);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER_PREVIEW_OFF", CommandFlags.Modal)]
    public static void GRASSHOPPER_PREVIEW_OFF()
    {
        if (CheckApplicationIsUnusable())
            return;

        var application = RhinoInsideAutoCadExtension.Application;
        var rhinoInsideManager = application!.RhinoInsideManager;
        var grasshopperPreview = rhinoInsideManager.GrasshopperPreviewServer;
        grasshopperPreview.SetMode(GrasshopperPreviewMode.Off);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER_PREVIEW_SHADED", CommandFlags.Modal)]
    public static void GRASSHOPPER_PREVIEW_SHADED()
    {
        if (CheckApplicationIsUnusable())
            return;

        var application = RhinoInsideAutoCadExtension.Application;
        var rhinoInsideManager = application!.RhinoInsideManager;
        var grasshopperPreview = rhinoInsideManager.GrasshopperPreviewServer;
        grasshopperPreview.SetMode(GrasshopperPreviewMode.Shaded);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER_PREVIEW_WIREFRAME", CommandFlags.Modal)]
    public static void GRASSHOPPER_PREVIEW_WIREFRAME()
    {
        if (CheckApplicationIsUnusable())
            return;

        var application = RhinoInsideAutoCadExtension.Application;
        var rhinoInsideManager = application!.RhinoInsideManager;
        var grasshopperPreview = rhinoInsideManager.GrasshopperPreviewServer;
        grasshopperPreview.SetMode(GrasshopperPreviewMode.Wireframe);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER_RECOMPUTE", CommandFlags.Modal)]
    public static void GRASSHOPPER_RECOMPUTE()
    {
        if (CheckApplicationIsUnusable())
            return;

        var application = RhinoInsideAutoCadExtension.Application;
        var rhinoInsideManager = application!.RhinoInsideManager;
        if (rhinoInsideManager.RhinoInstance.ActiveDoc == null) return;

        var grasshopperInstance = rhinoInsideManager.GrasshopperInstance;
        grasshopperInstance.RecomputeSolution();
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER_TOGGLE_SOLVER", CommandFlags.Modal)]
    public static void GRASSHOPPER_TOGGLE_SOLVER()
    {
        if (CheckApplicationIsUnusable())
            return;

        var application = RhinoInsideAutoCadExtension.Application;
        var rhinoInsideManager = application!.RhinoInsideManager;
        if (rhinoInsideManager.RhinoInstance.ActiveDoc == null) return;

        var grasshopperInstance = rhinoInsideManager.GrasshopperInstance;
        var isEnabled = grasshopperInstance.IsEnabled;

        if (isEnabled)
        {
            grasshopperInstance.DisableSolver();
        }
        else
        {
            grasshopperInstance.EnableSolver();
        }

        var buttonReplacer = new ButtonIconReplacer(_grasshopperSolverButtonId);
        var imagePath = isEnabled
            ? _grasshopperSolverOffIcon
            : _grasshopperSolverOnIcon;
        buttonReplacer.Replace(imagePath);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "OPEN_RHINO_VIEWPORT", CommandFlags.Modal)]
    public static void OPEN_RHINO_VIEWPORT()
    {
        if (_isLaunching || CheckApplicationIsUnusable())
            return;

        _isLaunching = true;
        var application = RhinoInsideAutoCadExtension.Application;
        var rhinoLauncher = new RhinoLauncher(application!);
        rhinoLauncher.Launch(RhinoInsideMode.Headless);
        var rhinoInstance = application!.RhinoInsideManager.RhinoInstance;
        rhinoInstance.RunRhinoScript(_newFloatingViewportScript);
        _isLaunching = false;
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO_PACKAGE_MANGER", CommandFlags.Modal)]
    public static void RHINO_PACKAGE_MANGER()
    {
        if (_isLaunching || CheckApplicationIsUnusable())
            return;

        _isLaunching = true;
        var application = RhinoInsideAutoCadExtension.Application;
        var rhinoLauncher = new RhinoLauncher(application!);
        rhinoLauncher.Launch(RhinoInsideMode.Headless);
        var rhinoInstance = application!.RhinoInsideManager.RhinoInstance;
        rhinoInstance.RunRhinoCommand(_packageManagerCommandName);
        _isLaunching = false;
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER_PLAYER", CommandFlags.Modal)]
    public static void GRASSHOPPER_PLAYER()
    {
        if (_isLaunching || CheckApplicationIsUnusable())
            return;

        _isLaunching = true;
        var application = RhinoInsideAutoCadExtension.Application;
        var rhinoLauncher = new RhinoLauncher(application!);
        rhinoLauncher.Launch(RhinoInsideMode.Headless);
        var rhinoInstance = application!.RhinoInsideManager.RhinoInstance;
        rhinoInstance.RunRhinoCommand(_grasshopperPlayerCommandName);
        _isLaunching = false;
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO_INSIDE_ABOUT", CommandFlags.Modal)]
    public static void RHINO_INSIDE_ABOUT()
    {
        if (CheckApplicationIsUnusable())
            return;

        var application = RhinoInsideAutoCadExtension.Application;
        application!.SupportDialogManager.Show(SupportDialogTab.About);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO_INSIDE_SUPPORT", CommandFlags.Modal)]
    public static void RHINO_INSIDE_SUPPORT()
    {
        if (CheckApplicationIsUnusable())
            return;

        var application = RhinoInsideAutoCadExtension.Application;
        application!.SupportDialogManager.Show(SupportDialogTab.Support);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO_INSIDE_UPDATE", CommandFlags.Modal)]
    public static void RHINO_INSIDE_UPDATE()
    {
        if (CheckApplicationIsUnusable())
            return;

        var application = RhinoInsideAutoCadExtension.Application;
        application!.SupportDialogManager.Show(SupportDialogTab.Update);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO_INSIDE_SETTINGS", CommandFlags.Modal)]
    public static void RHINO_INSIDE_SETTINGS()
    {
        if (CheckApplicationIsUnusable())
            return;

        var application = RhinoInsideAutoCadExtension.Application;
        application!.SupportDialogManager.Show(SupportDialogTab.Settings);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO_INSIDE_CONVERT_BREP", CommandFlags.Transparent)]
    public static void RHINO_INSIDE_CONVERT_BREP()
    {
        if (CheckApplicationIsUnusable())
            return;

        var application = RhinoInsideAutoCadExtension.Application;
        var brepConverterRunner = application!.BrepConverterRunner;
        brepConverterRunner.Run();
    }
}
