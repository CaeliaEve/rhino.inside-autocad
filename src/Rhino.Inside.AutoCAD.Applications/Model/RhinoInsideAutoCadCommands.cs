using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;

    private static void LaunchOrActivateStandaloneRhino(int majorVersion, bool launchGrasshopper = false)
    {
        var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;

        try
        {
            var locator = new RhinoInstallationLocator();
            var installations = locator.Locate();
            var targetInstall = installations.FirstOrDefault(i => i.MajorVersion == majorVersion)
                             ?? (majorVersion == 8 ? installations.FirstOrDefault(i => i.MajorVersion >= 8) : installations.FirstOrDefault());

            if (targetInstall == null || string.IsNullOrEmpty(targetInstall.SystemDirectory))
            {
                editor?.WriteMessage($"\n[Rhino Launcher] Could not find Rhino {majorVersion} installation in registry.\n");
                return;
            }

            var rhinoExePath = Path.Combine(targetInstall.SystemDirectory, "Rhino.exe");
            if (!File.Exists(rhinoExePath))
            {
                editor?.WriteMessage($"\n[Rhino Launcher] Rhino.exe not found at: {rhinoExePath}\n");
                return;
            }

            // Check if standalone Rhino is already running and bring it to foreground
            var runningProcs = Process.GetProcessesByName("Rhino");
            foreach (var proc in runningProcs)
            {
                try
                {
                    if (proc.MainModule?.FileName != null &&
                        proc.MainModule.FileName.StartsWith(targetInstall.SystemDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        var handle = proc.MainWindowHandle;
                        if (handle != IntPtr.Zero)
                        {
                            ShowWindowAsync(handle, SW_RESTORE);
                            SetForegroundWindow(handle);
                            editor?.WriteMessage($"\n[Rhino Launcher] Activated standalone Rhino {majorVersion} (PID: {proc.Id}).\n");
                            return;
                        }
                    }
                }
                catch { }
            }

            // Launch standalone process detached from AutoCAD via Windows shell
            var netfxArg = majorVersion >= 8 ? "/netfx " : "";
            var scriptArg = launchGrasshopper ? "/runscript=\"-Grasshopper\"" : "";
            var fullArgs = (netfxArg + scriptArg).Trim();

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c start \"\" \"{rhinoExePath}\" {fullArgs}",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = targetInstall.SystemDirectory
            };

            // Show the official Rhino.Inside.AutoCAD splash screen animation
            var splash = new Rhino.Inside.AutoCAD.UI.Resources.Models.LoadingScreenManager("1.0.0", $"{majorVersion}.0");
            splash.Show();

            Process.Start(startInfo);
            editor?.WriteMessage($"\n[Rhino Launcher] Launched detached standalone Rhino {majorVersion}.\n");

            // Automatically close splash when IPC client connects or after timeout
            System.Threading.Tasks.Task.Run(async () =>
            {
                for (int i = 0; i < 40; i++)
                {
                    await System.Threading.Tasks.Task.Delay(250);
                    if (Rhino.Inside.AutoCAD.Core.IPC.LiveLinkManager.Instance.IsClientConnected) break;
                }
                await System.Threading.Tasks.Task.Delay(1000);
                splash.Close();
            });
        }
        catch (System.Exception ex)
        {
            editor?.WriteMessage($"\n[Rhino Launcher] Launch failed: {ex.Message}\n");
        }
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO", CommandFlags.Modal)]
    public static void RHINO()
    {
        LaunchOrActivateStandaloneRhino(8, false);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO7", CommandFlags.Modal)]
    public static void RHINO7()
    {
        LaunchOrActivateStandaloneRhino(7, false);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO8", CommandFlags.Modal)]
    public static void RHINO8()
    {
        LaunchOrActivateStandaloneRhino(8, false);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER", CommandFlags.Modal)]
    public static void GRASSHOPPER()
    {
        LaunchOrActivateStandaloneRhino(8, true);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GH7", CommandFlags.Modal)]
    public static void GH7()
    {
        LaunchOrActivateStandaloneRhino(7, true);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GH8", CommandFlags.Modal)]
    public static void GH8()
    {
        LaunchOrActivateStandaloneRhino(8, true);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO_INSIDE", CommandFlags.Modal)]
    public static void RHINO_INSIDE()
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

    [CommandMethod("RHINOINSIDE_COMMANDS", "GH_INSIDE", CommandFlags.Modal)]
    public static void GH_INSIDE()
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
        LaunchOrActivateStandaloneRhino(8, false);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINO_PACKAGE_MANGER", CommandFlags.Modal)]
    public static void RHINO_PACKAGE_MANGER()
    {
        LaunchOrActivateStandaloneRhino(8, false);
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "GRASSHOPPER_PLAYER", CommandFlags.Modal)]
    public static void GRASSHOPPER_PLAYER()
    {
        LaunchOrActivateStandaloneRhino(8, true);
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

    [CommandMethod("RHINOINSIDE_COMMANDS", "TOGGLE_RHINO_LINK", CommandFlags.Modal)]
    public static void TOGGLE_RHINO_LINK()
    {
        var newState = Rhino.Inside.AutoCAD.Core.IPC.LiveLinkManager.Instance.ToggleLiveLink();
        var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;
        editor?.WriteMessage(newState 
            ? "\n[Rhino Live Link] 🟢 Live Link: ON (IPC Bridge Active)\n" 
            : "\n[Rhino Live Link] ⚪ Live Link: OFF (IPC Bridge Suspended)\n");
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "BAKE_ALL_TO_CAD", CommandFlags.Modal)]
    public static void BAKE_ALL_TO_CAD()
    {
        var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;
        if (!Rhino.Inside.AutoCAD.Core.IPC.LiveLinkManager.Instance.IsEnabled)
        {
            editor?.WriteMessage("\n[Rhino Live Link] Please turn Live Link ON to bake objects.\n");
            return;
        }

        _ = Rhino.Inside.AutoCAD.Core.IPC.LiveLinkManager.Instance.SendMessageAsync(
            Rhino.Inside.AutoCAD.Core.IPC.IpcMessage.Create(Rhino.Inside.AutoCAD.Core.IPC.IpcCommandType.BakeRequest, "BakeAll"));
        editor?.WriteMessage("\n[Rhino Live Link] Dispatched Bake command to Grasshopper.\n");
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "SYNC_RHINO_LINK", CommandFlags.Modal)]
    public static void SYNC_RHINO_LINK()
    {
        var isConnected = Rhino.Inside.AutoCAD.Core.IPC.LiveLinkManager.Instance.IsClientConnected;
        var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;
        editor?.WriteMessage(isConnected 
            ? "\n[Rhino Live Link] Connection status: Connected to Rhino 8.\n" 
            : "\n[Rhino Live Link] Connection status: Listening for Rhino 8...\n");
    }

    [CommandMethod("RHINOINSIDE_COMMANDS", "RHINOINSIDE_INTERNAL_SELECT", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
    public static void RHINOINSIDE_INTERNAL_SELECT()
    {
        Rhino.Inside.AutoCAD.Applications.IPC.LiveLinkServerHandler.ExecuteSelectionInCommandContext();
    }
}
