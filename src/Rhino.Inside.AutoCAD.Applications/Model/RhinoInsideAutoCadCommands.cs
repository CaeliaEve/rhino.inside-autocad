using Autodesk.AutoCAD.Runtime;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Interop;
using Rhino.Inside.AutoCAD.Services;
using Rhino.Inside.AutoCAD.UI.Resources.Models;

[assembly: CommandClass(typeof(RhinoInsideAutoCadCommands))]

namespace Rhino.Inside.AutoCAD.Applications;

/// <summary>
/// The commands class for Rhino.Inside.AutoCAD application commands.
/// Provides native in-process execution with single-session mutual exclusion between Rhino 7 and Rhino 8.
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
    private static bool CheckApplicationIsUnusable()
    {
        if (RhinoInsideAutoCadExtension.LoadFailureMessage is { } loadFailureMessage)
        {
            Autodesk.AutoCAD.ApplicationServices.Core.Application
                .ShowAlertDialog(loadFailureMessage);

            return true;
        }

        if (RhinoInsideAutoCadExtension.Application is null)
        {
            if (!RhinoInsideAutoCadExtension.EnsureInitialized())
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
        if (_isLaunching || !RhinoInsideAutoCadExtension.EnsureInitialized())
            return;

        _isLaunching = true;
        try
        {
            var application = RhinoInsideAutoCadExtension.Application;
            var rhinoLauncher = new RhinoLauncher(application!);
            rhinoLauncher.Launch(RhinoInsideMode.Windowed);
        }
        finally
        {
            _isLaunching = false;
        }
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO7", CommandFlags.Modal)]
    public static void RHINO7()
    {
        if (_isLaunching || !RhinoInsideAutoCadExtension.EnsureInitialized(7))
            return;

        _isLaunching = true;
        try
        {
            var application = RhinoInsideAutoCadExtension.Application;
            var rhinoLauncher = new RhinoLauncher(application!);
            rhinoLauncher.Launch(RhinoInsideMode.Windowed);
        }
        finally
        {
            _isLaunching = false;
        }
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO8", CommandFlags.Modal)]
    public static void RHINO8()
    {
        if (_isLaunching || !RhinoInsideAutoCadExtension.EnsureInitialized(8))
            return;

        _isLaunching = true;
        try
        {
            var application = RhinoInsideAutoCadExtension.Application;
            var rhinoLauncher = new RhinoLauncher(application!);
            rhinoLauncher.Launch(RhinoInsideMode.Windowed);
        }
        finally
        {
            _isLaunching = false;
        }
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER", CommandFlags.Modal)]
    public static void GRASSHOPPER()
    {
        if (_isLaunching || !RhinoInsideAutoCadExtension.EnsureInitialized())
            return;

        _isLaunching = true;
        try
        {
            var application = RhinoInsideAutoCadExtension.Application;
            var rhinoLauncher = new RhinoLauncher(application!);
            rhinoLauncher.Launch(RhinoInsideMode.Headless);
            var rhinoInstance = application!.RhinoInsideManager.RhinoInstance;
            rhinoInstance.RunRhinoCommand(_grasshopperCommandName);
        }
        finally
        {
            _isLaunching = false;
        }
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GH7", CommandFlags.Modal)]
    public static void GH7()
    {
        if (_isLaunching || !RhinoInsideAutoCadExtension.EnsureInitialized(7))
            return;

        _isLaunching = true;
        try
        {
            var application = RhinoInsideAutoCadExtension.Application;
            var rhinoLauncher = new RhinoLauncher(application!);
            rhinoLauncher.Launch(RhinoInsideMode.Headless);
            var rhinoInstance = application!.RhinoInsideManager.RhinoInstance;
            rhinoInstance.RunRhinoCommand(_grasshopperCommandName);
        }
        finally
        {
            _isLaunching = false;
        }
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GH8", CommandFlags.Modal)]
    public static void GH8()
    {
        if (_isLaunching || !RhinoInsideAutoCadExtension.EnsureInitialized(8))
            return;

        _isLaunching = true;
        try
        {
            var application = RhinoInsideAutoCadExtension.Application;
            var rhinoLauncher = new RhinoLauncher(application!);
            rhinoLauncher.Launch(RhinoInsideMode.Headless);
            var rhinoInstance = application!.RhinoInsideManager.RhinoInstance;
            rhinoInstance.RunRhinoCommand(_grasshopperCommandName);
        }
        finally
        {
            _isLaunching = false;
        }
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "SWITCH_RHINO_VERSION", CommandFlags.Modal)]
    public static void SWITCH_RHINO_VERSION()
    {
        RhinoInsideAutoCadExtension.PromptSwitchVersion();
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "TOGGLE_RHINO_PREVIEW", CommandFlags.Modal)]
    public static void TOGGLE_RHINO_PREVIEW()
    {
        try
        {
            if (CheckApplicationIsUnusable())
                return;

            var application = RhinoInsideAutoCadExtension.Application;
            var rhinoInsideManager = application?.RhinoInsideManager;
            if (rhinoInsideManager == null) return;

            var rhinoObjectPreview = rhinoInsideManager.RhinoPreviewServer;
            rhinoObjectPreview.ToggleVisibility();

            var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;
            editor?.WriteMessage(rhinoObjectPreview.Visible ? "\n[Rhino.Inside] Rhino Preview: ON\n" : "\n[Rhino.Inside] Rhino Preview: OFF\n");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error toggling preview: " + ex.Message);
        }
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER_PREVIEW_OFF", CommandFlags.Modal)]
    public static void GRASSHOPPER_PREVIEW_OFF()
    {
        try
        {
            if (CheckApplicationIsUnusable())
                return;

            var application = RhinoInsideAutoCadExtension.Application;
            var rhinoInsideManager = application?.RhinoInsideManager;
            if (rhinoInsideManager == null) return;

            var grasshopperPreview = rhinoInsideManager.GrasshopperPreviewServer;
            grasshopperPreview.SetMode(GrasshopperPreviewMode.Off);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error setting preview off: " + ex.Message);
        }
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER_PREVIEW_SHADED", CommandFlags.Modal)]
    public static void GRASSHOPPER_PREVIEW_SHADED()
    {
        try
        {
            if (CheckApplicationIsUnusable())
                return;

            var application = RhinoInsideAutoCadExtension.Application;
            var rhinoInsideManager = application?.RhinoInsideManager;
            if (rhinoInsideManager == null) return;

            var grasshopperPreview = rhinoInsideManager.GrasshopperPreviewServer;
            grasshopperPreview.SetMode(GrasshopperPreviewMode.Shaded);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error setting preview shaded: " + ex.Message);
        }
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER_PREVIEW_WIREFRAME", CommandFlags.Modal)]
    public static void GRASSHOPPER_PREVIEW_WIREFRAME()
    {
        try
        {
            if (CheckApplicationIsUnusable())
                return;

            var application = RhinoInsideAutoCadExtension.Application;
            var rhinoInsideManager = application?.RhinoInsideManager;
            if (rhinoInsideManager == null) return;

            var grasshopperPreview = rhinoInsideManager.GrasshopperPreviewServer;
            grasshopperPreview.SetMode(GrasshopperPreviewMode.Wireframe);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error setting preview wireframe: " + ex.Message);
        }
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER_RECOMPUTE", CommandFlags.Modal)]
    public static void GRASSHOPPER_RECOMPUTE()
    {
        try
        {
            if (CheckApplicationIsUnusable())
                return;

            var application = RhinoInsideAutoCadExtension.Application;
            var rhinoInsideManager = application?.RhinoInsideManager;
            if (rhinoInsideManager?.RhinoInstance.ActiveDoc == null) return;

            var grasshopperInstance = rhinoInsideManager.GrasshopperInstance;
            grasshopperInstance?.RecomputeSolution();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error recomputing: " + ex.Message);
        }
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER_TOGGLE_SOLVER", CommandFlags.Modal)]
    public static void GRASSHOPPER_TOGGLE_SOLVER()
    {
        try
        {
            if (CheckApplicationIsUnusable())
                return;

            var application = RhinoInsideAutoCadExtension.Application;
            var rhinoInsideManager = application?.RhinoInsideManager;
            if (rhinoInsideManager?.RhinoInstance.ActiveDoc == null) return;

            var grasshopperInstance = rhinoInsideManager.GrasshopperInstance;
            if (grasshopperInstance == null) return;

            var isEnabled = grasshopperInstance.IsEnabled;

            if (isEnabled)
            {
                grasshopperInstance.DisableSolver();
            }
            else
            {
                grasshopperInstance.EnableSolver();
            }

            var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;
            editor?.WriteMessage(isEnabled ? "\n[Rhino.Inside] Grasshopper Solver: OFF (Locked)\n" : "\n[Rhino.Inside] Grasshopper Solver: ON (Running)\n");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error toggling solver: " + ex.Message);
        }
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "OPEN_RHINO_VIEWPORT", CommandFlags.Modal)]
    public static void OPEN_RHINO_VIEWPORT()
    {
        if (_isLaunching || CheckApplicationIsUnusable())
            return;

        _isLaunching = true;
        try
        {
            var application = RhinoInsideAutoCadExtension.Application;
            var rhinoLauncher = new RhinoLauncher(application!);
            rhinoLauncher.Launch(RhinoInsideMode.Headless);
            var rhinoInstance = application!.RhinoInsideManager.RhinoInstance;
            rhinoInstance.RunRhinoScript(_newFloatingViewportScript);
        }
        finally
        {
            _isLaunching = false;
        }
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO_PACKAGE_MANGER", CommandFlags.Modal)]
    public static void RHINO_PACKAGE_MANGER()
    {
        if (_isLaunching || CheckApplicationIsUnusable())
            return;

        _isLaunching = true;
        try
        {
            var application = RhinoInsideAutoCadExtension.Application;
            var rhinoLauncher = new RhinoLauncher(application!);
            rhinoLauncher.Launch(RhinoInsideMode.Headless);
            var rhinoInstance = application!.RhinoInsideManager.RhinoInstance;
            rhinoInstance.RunRhinoCommand(_packageManagerCommandName);
        }
        finally
        {
            _isLaunching = false;
        }
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER_PLAYER", CommandFlags.Modal)]
    public static void GRASSHOPPER_PLAYER()
    {
        if (_isLaunching || CheckApplicationIsUnusable())
            return;

        _isLaunching = true;
        try
        {
            var application = RhinoInsideAutoCadExtension.Application;
            var rhinoLauncher = new RhinoLauncher(application!);
            rhinoLauncher.Launch(RhinoInsideMode.Headless);
            var rhinoInstance = application!.RhinoInsideManager.RhinoInstance;
            rhinoInstance.RunRhinoCommand(_grasshopperPlayerCommandName);
        }
        finally
        {
            _isLaunching = false;
        }
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

    [CommandMethod("RHINOINSIDE_COMMANDS", "SHOW_RHINO_RIBBON", CommandFlags.Modal)]
    public static void SHOW_RHINO_RIBBON()
    {
        Rhino.Inside.AutoCAD.Applications.UI.RibbonBuilder.Initialize();
        var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;
        editor?.WriteMessage("\n[Rhino.Inside] Ribbon tab refreshed.\n");
    }
}
