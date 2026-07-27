using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using Rhino.Inside.AutoCAD.Services;
using Rhino.Inside.AutoCAD.UI.Resources.Models;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Rhino.Inside.AutoCAD.Applications;

/// <inheritdoc cref="IRhinoInsideAutoCadApplication"/>
public class RhinoInsideAutoCadApplication : IRhinoInsideAutoCadApplication
{
    private readonly IList<string> _materialDesignAssemblyNames = ApplicationConstants.MaterialDesignAssemblyNames;

    /// <inheritdoc/>
    public ISettingsManager SettingsManager { get; }

    /// <inheritdoc/>
    public IBootstrapper Bootstrapper { get; }

    /// <inheritdoc/>
    public IApplicationConfig ApplicationConfig { get; }

    /// <inheritdoc/>
    public IRhinoInsideManager RhinoInsideManager { get; }

    /// <inheritdoc/>
    public ISupportDialogManager SupportDialogManager { get; }

    /// <inheritdoc/>
    public IBrepConverterRunner BrepConverterRunner { get; }

    /// <summary>
    /// Constructs a new <see cref="IRhinoInsideAutoCadApplication"/> from an already
    /// bootstrapped application.
    /// </summary>
    /// <remarks>
    /// Constructing this touches RhinoCommon, so the caller must already have bound the app
    /// domain to a Rhino installation with <see cref="RhinoCoreExtension.BindTo"/>. The
    /// plugin does not load at all when that is not possible, which is why nothing here is
    /// conditional on Rhino being present.
    /// </remarks>
    /// <param name="bootstrapper">The bootstrapper for the host application.</param>
    /// <param name="applicationConfig">The application configuration settings.</param>
    public RhinoInsideAutoCadApplication(IBootstrapper bootstrapper,
        IApplicationConfig applicationConfig)
    {
        var applicationDirectories = bootstrapper.InstallationDirectories;

        this.SettingsManager = new SettingManager(applicationDirectories);

        this.Bootstrapper = bootstrapper;

        this.ApplicationConfig = applicationConfig;

        var rhinoInstance = new RhinoInstance(applicationDirectories);

        var autocadInstance = new AutoCadInstance(bootstrapper.Dispatcher);

        var grasshopperInstance = new GrasshopperInstance(applicationDirectories,
            autocadInstance.IsCivil3d);

        this.RhinoInsideManager = new RhinoInsideManager(rhinoInstance, grasshopperInstance,
            autocadInstance, this.SettingsManager.User.Settings);

        this.BrepConverterRunner = new BrepConverterRunner();

        this.LoadMaterialDesign(applicationDirectories);

        this.SupportDialogManager = new SupportDialogManager(this);
    }

    /// <summary>
    /// The Material Design library has to be force loaded into Revit to avoid runtime
    /// exceptions as it's not automatically loaded as the calls to the library are always
    /// from XAML. This method guarantees its loaded.
    /// </summary>
    private void LoadMaterialDesign(IInstallationDirectories installationDirectories)
    {
        foreach (var names in _materialDesignAssemblyNames)
        {
            var assemblyPath = Path.Combine(installationDirectories.VersionedAssemblies, names);
            var assemblyName = AssemblyName.GetAssemblyName(assemblyPath);

            Assembly.Load(assemblyName);
        }
    }

    /// <inheritdoc />
    public void ShowAlertDialog(string message)
    {
        Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowAlertDialog(message);
    }

    /// <inheritdoc />
    public void Terminate()
    {
        this.RhinoInsideManager?.Shutdown();

        this.SupportDialogManager?.Dispose();

        this.Bootstrapper?.AssemblyResolver.Terminate();

        LoggerService.Instance?.Shutdown();
    }
}
