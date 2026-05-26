using Autodesk.AutoCAD.Runtime;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Core.State;
using Rhino.Inside.AutoCAD.Interop;
using Rhino.Inside.AutoCAD.Services;
using System.Globalization;
using System.Reflection;

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

    /// <summary>
    /// The singleton instance of the <see cref="IRhinoInsideAutoCadApplication"/>
    /// </summary>
    public static IRhinoInsideAutoCadApplication? Application { get; private set; }

    /// <summary>
    /// Initialize the <see cref="IRhinoInsideAutoCadApplication"/>
    /// </summary>
    public void Initialize()
    {

        var editor = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument?.Editor;

        var currentDate = System.DateTime.Now;

        try
        {

            var compliedDate = this.GetCompliedDate();

            var limitDate = compliedDate.AddDays(180);

            if (currentDate > limitDate)
            {
                editor?.WriteMessage(_expiredMessage);

                Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowAlertDialog(_expiredMessage);

                return;
            }

#if DEBUGNET8 || DEGBUG
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in loadedAssemblies)
            {

                editor?.WriteMessage($"Already loaded: {asm.FullName}\n");
                editor?.WriteMessage($"From: {asm.Location}\n");

            }
#endif

            // Force RhinoCoreExtension static constructor to run first
            // This sets up the AssemblyResolve handler for RhinoCommon before
            // any code tries to reference RhinoCommon types
            _ = RhinoCoreExtension.Instance;

            Application = new RhinoInsideAutoCadApplication();

            Autodesk.AutoCAD.ApplicationServices.Core.Application.BeginQuit += this.OnApplicationBeginQuit;

            editor?.WriteMessage(_applicationLoadedSuccessMessage);
        }
        catch (System.Exception e)
        {
            var message = string.Format(_applicationLoadErrorMessageFormat, e.Message);

            editor?.WriteMessage(message);
            editor?.WriteMessage(string.Format(_stackTraceMessageFormat, e.StackTrace));
            editor?.WriteMessage(Assembly.GetExecutingAssembly().Location.ToString());

            Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowAlertDialog(message);

            throw;
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

    /// <summary>
    /// Safely terminates the <see cref="IRhinoInsideAutoCadApplication"/> by saving
    /// any edited rhino or grasshopper files and catching any exceptions that may
    /// occur during termination. This ensures  that the application can attempt to
    /// terminate without crashing, even if  there are issues during the termination
    /// process.
    /// </summary>
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

    /// <summary>
    /// Subscribes to the AutoCAD application quit event to ensure that the
    /// <see cref="IRhinoInsideAutoCadApplication"/> is properly terminated.
    /// </summary>
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

    /// <summary>
    /// Terminate the <see cref="IRhinoInsideAutoCadApplication"/>
    /// </summary>
    public void Terminate()
    {
        Autodesk.AutoCAD.ApplicationServices.Core.Application.BeginQuit -= this.OnApplicationBeginQuit;
    }
}
