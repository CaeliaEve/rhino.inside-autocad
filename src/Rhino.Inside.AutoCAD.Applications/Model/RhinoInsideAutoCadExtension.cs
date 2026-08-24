using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Core.State;
using Rhino.Inside.AutoCAD.Interop;
using Rhino.Inside.AutoCAD.Services;
using Rhino.Inside.AutoCAD.UI.Resources.Models;

[assembly: ExtensionApplication(typeof(RhinoInsideAutoCadExtension))]

namespace Rhino.Inside.AutoCAD.Applications;

/// <inheritdoc cref="IRhinoInsideAutoCadApplication"/>
public class RhinoInsideAutoCadExtension : IExtensionApplication
{
    private const string _applicationLoadedSuccessMessage = ApplicationConstants.ApplicationLoadedSuccessMessage;
    private const string _applicationLoadErrorMessageFormat = ApplicationConstants.ApplicationLoadErrorMessageFormat;
    private const string _stackTraceMessageFormat = ApplicationConstants.StackTraceMessageFormat;
    private const string _expiredMessage = ApplicationConstants.ExpiredMessage;
    private const string _buildVersionMetadataPrefix = ApplicationConstants.BuildVersionMetadataPrefix;
    private const string _rhinoNotInstalledErrorMessage = ApplicationConstants.RhinoNotInstalledErrorMessage;
    private const string _rhinoVersionNotSelectedErrorMessage = ApplicationConstants.RhinoVersionNotSelectedErrorMessage;
    private const string _applicationLoadAbortedMessageFormat = ApplicationConstants.ApplicationLoadAbortedMessageFormat;

    private static Bootstrapper? _bootstrapper;
    private static RhinoInsideAutoCadApplicationConfig? _applicationConfig;
    private static readonly object _initLock = new();

    /// <summary>
    /// The singleton instance of the <see cref="IRhinoInsideAutoCadApplication"/>
    /// </summary>
    public static IRhinoInsideAutoCadApplication? Application { get; private set; }

    /// <summary>
    /// Indicates whether the application has expired.
    /// </summary>
    public static bool IsExpired { get; private set; }

    /// <summary>
    /// The reason the plugin did not finish loading, or null if it did.
    /// </summary>
    public static string? LoadFailureMessage { get; private set; }

    /// <summary>
    /// Initialize the <see cref="IRhinoInsideAutoCadApplication"/> in on-demand lazy mode.
    /// </summary>
    public void Initialize()
    {
        var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;
        var currentDate = DateTime.Now;

        try
        {
            var compliedDate = this.GetCompliedDate();
            var limitDate = compliedDate.AddDays(180);

            if (currentDate > limitDate)
            {
                IsExpired = true;
            }

            // Bootstrap logger and application configuration (0ms startup overhead)
            _applicationConfig = new RhinoInsideAutoCadApplicationConfig();
            _bootstrapper = new Bootstrapper(new AutocadBootstrapperConfig(_applicationConfig));

            RhinoCoreExtension.Instance.StartUpLogger.Flush();

            // Programmatically inject and guarantee Ribbon Tab on any workspace
            Rhino.Inside.AutoCAD.Applications.UI.RibbonBuilder.Initialize();

            // Initialize Live Link IPC Server handler
            Rhino.Inside.AutoCAD.Applications.IPC.LiveLinkServerHandler.Initialize();

            Autodesk.AutoCAD.ApplicationServices.Core.Application.BeginQuit += this.OnApplicationBeginQuit;

            editor?.WriteMessage("\n[Rhino.Inside] Plugin loaded (On-demand mode ready). Run RHINO, RHINO7, or RHINO8 to start.\n");
        }
        catch (System.Exception e)
        {
            var message = string.Format(_applicationLoadErrorMessageFormat, e.Message);

            editor?.WriteMessage(message);
            editor?.WriteMessage(string.Format(_stackTraceMessageFormat, e.StackTrace));
            editor?.WriteMessage(Assembly.GetExecutingAssembly().Location);

            Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowAlertDialog(message);
            throw;
        }
    }

    /// <summary>
    /// Ensures that Rhino is bound and initialized natively, with strict single-session mutual exclusion.
    /// </summary>
    public static bool EnsureInitialized(int? targetVersion = null)
    {
        if (Application != null)
        {
            var currentVersion = RhinoCoreExtension.SelectedInstallation?.MajorVersion;
            if (targetVersion.HasValue && currentVersion.HasValue && currentVersion.Value != targetVersion.Value)
            {
                var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;
                var msg = string.Format(
                    "Rhino {0} is currently active in this session.\n\nTo use Rhino {1}, please restart AutoCAD and run RHINO{1}.",
                    currentVersion.Value,
                    targetVersion.Value);

                editor?.WriteMessage(string.Format("\n[Rhino.Inside] {0}\n", msg));
                Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowAlertDialog(msg);
                return false;
            }
            return true;
        }

        lock (_initLock)
        {
            if (Application != null) return true;

            var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;
            var installationLocator = new RhinoInstallationLocator();
            var userSettingsStore = UserSettingsStore.Instance;
            var versionDialogManager = new RhinoVersionDialogManager();

            IRhinoInstallation? installation = null;

            if (targetVersion.HasValue)
            {
                var allInstalls = installationLocator.Locate();
                installation = allInstalls.FirstOrDefault(x => x.MajorVersion == targetVersion.Value);
            }

            if (installation == null)
            {
                var versionSelection = new RhinoVersionSelection(installationLocator,
                    userSettingsStore, versionDialogManager);
                installation = versionSelection.Resolve(out var anyVersionInstalled);

                if (installation is null)
                {
                    AbortLoadStatic(editor, anyVersionInstalled);
                    return false;
                }
            }

            try
            {
                RhinoCoreExtension.BindTo(installation);

                if (_bootstrapper != null && _applicationConfig != null)
                {
                    Application = new RhinoInsideAutoCadApplication(_bootstrapper, _applicationConfig);
                }

                editor?.WriteMessage(string.Format("\n[Rhino.Inside] Bound to {0} successfully.\n", installation.DisplayName));
                return true;
            }
            catch (System.Exception ex)
            {
                editor?.WriteMessage(string.Format("\n[Rhino.Inside] Initialization error: {0}\n", ex.Message));
                Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowAlertDialog(string.Format("[Rhino.Inside] Initialization error: {0}\n{1}", ex.Message, ex.StackTrace));
                return false;
            }
        }
    }

    /// <summary>
    /// Displays version dialog or version lock explanation.
    /// </summary>
    public static void PromptSwitchVersion()
    {
        if (Application != null)
        {
            var currentVer = RhinoCoreExtension.SelectedInstallation?.MajorVersion ?? 0;
            var otherVer = currentVer == 7 ? 8 : 7;
            var msg = string.Format(
                "Rhino {0} is currently active in this session.\n\nTo use Rhino {1}, please restart AutoCAD and run RHINO{1}.",
                currentVer,
                otherVer);

            var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;
            editor?.WriteMessage(string.Format("\n[Rhino.Inside] {0}\n", msg));
            Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowAlertDialog(msg);
            return;
        }

        EnsureInitialized();
    }

    private static void AbortLoadStatic(Autodesk.AutoCAD.EditorInput.Editor? editor, bool anyVersionInstalled)
    {
        if (!anyVersionInstalled)
        {
            LoadFailureMessage = _rhinoNotInstalledErrorMessage;
            editor?.WriteMessage(string.Format(_applicationLoadAbortedMessageFormat, LoadFailureMessage));
        }
        else
        {
            editor?.WriteMessage("\n[Rhino.Inside] Rhino version selection was cancelled.\n");
        }
    }

    /// <summary>
    /// Returns the date the assembly was compiled.
    /// </summary>
    private DateTime GetCompliedDate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(_buildVersionMetadataPrefix);
            if (index > 0)
            {
                value = value.Substring(index + _buildVersionMetadataPrefix.Length);
                if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    return result;
                }
            }
        }

        return default;
    }

    private void SafeTermination()
    {
        try
        {
            Application?.Terminate();
            Application = null;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Shutdown error: {ex.Message}");
            try { LoggerService.Instance?.LogError(ex, "Termination error"); } catch { }
            Application = null;
        }
    }

    private void OnApplicationBeginQuit(object sender, Autodesk.AutoCAD.ApplicationServices.BeginQuitEventArgs e)
    {
        if (Application is not null)
        {
            e.IsVetoed = true;
            ApplicationState.BeginShutdown();
            Autodesk.AutoCAD.ApplicationServices.Core.Application.BeginQuit -= this.OnApplicationBeginQuit;
            this.SafeTermination();
            Autodesk.AutoCAD.ApplicationServices.Core.Application.Quit();
        }
    }

    public void Terminate()
    {
        Autodesk.AutoCAD.ApplicationServices.Core.Application.BeginQuit -= this.OnApplicationBeginQuit;
    }
}
