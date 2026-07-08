using Autodesk.AutoCAD.DatabaseServices;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Services;
using Rhino.Runtime.InProcess;
using System.Reflection;

namespace Rhino.Inside.AutoCAD.Interop;

/// <inheritdoc cref="IRhinoCoreExtension"/>
public class RhinoCoreExtension : IRhinoCoreExtension
{
    private const string _rhinoRegistryKeyPath = ApplicationConstants.RhinoRegistryKeyPath;
    private const string _rhinoInstallPathValueName = ApplicationConstants.RhinoInstallPathValueName;
    private const string _rhinoPluginsFolderValueName = ApplicationConstants.RhinoPluginsFolderValueName;
    private const string _rhinoCommonAssemblyName = ApplicationConstants.RhinoCommonAssemblyName;
    private const string _grasshopperAssemblyName = ApplicationConstants.GrasshopperAssemblyName;
    private const string _grasshopperIoAssemblyName = ApplicationConstants.GrasshopperIOAssemblyName;
    private const string _rhinoCommonDllName = ApplicationConstants.RhinoCommonDllName;
    private const string _grasshopperDllRelativePath = ApplicationConstants.GrasshopperDllRelativePath;
    private const string _grasshopperIoDllRelativePath = ApplicationConstants.GrasshopperIoDllRelativePath;
    private const string _rhinoNoSplashArgument = ApplicationConstants.RhinoNoSplashArgument;
    private const string _rhinoSchemeArgumentFormat = ApplicationConstants.RhinoSchemeArgumentFormat;
    private const string _rhinoInsideSchemeNameFormat = ApplicationConstants.RhinoInsideSchemeNameFormat;
    private const string _rhinoNotInstalledErrorMessage = ApplicationConstants.RhinoNotInstalledErrorMessage;
    private const string _rhinoCoreInitializationFailedErrorMessage = ApplicationConstants.RhinoCoreInitializationFailedErrorMessage;
    private const string _wcfErrorMessage = Services.MessageConstants.WcfErrorMessage;
    private const string _systemPrimitiveDll = ApplicationConstants.SystemPrimitiveDll;
    private const string _systemHttpDll = ApplicationConstants.SystemHttpDll;
    private const string _serviceModelFamliy8_0 = ApplicationConstants.ServiceModelFamliy8_0;
    private const string _serviceModelFamliy8_1 = ApplicationConstants.ServiceModelFamliy8_1;
    private const string _serviceModelFamliy6_0 = ApplicationConstants.ServiceModelFamliy6_0;

    private static RhinoCore? _rhinoCore;

    /// <summary>
    /// True if Rhino is installed otherwise false.
    /// </summary>
    private static readonly bool _rhinoInstallDirectoryExists;

    /// <summary>
    /// The <see cref="RhinoCoreExtension"/> singleton instance.
    /// </summary>
    public static RhinoCoreExtension Instance { get; }

    /// <inheritdoc />
    public IStartUpLogger StartUpLogger { get; }

    /// <inheritdoc />
    public IRhinoWindowManager WindowManager { get; }

    /// <summary>
    /// Gets the Rhino system directory in the local machines registry.
    /// </summary>
    static readonly string _systemDir = (string)Microsoft.Win32.Registry.GetValue
    (
        _rhinoRegistryKeyPath, _rhinoInstallPathValueName, string.Empty
    );

    /// <summary>
    /// Gets the Rhino system directory in the local machines registry.
    /// </summary>
    static readonly string _pluginDir = (string)Microsoft.Win32.Registry.GetValue
    (
        _rhinoRegistryKeyPath, _rhinoPluginsFolderValueName, string.Empty
    );

    /// <summary>
    /// Constructs a new <see cref="RhinoCoreExtension"/> instance.
    /// </summary>
    private RhinoCoreExtension()
    {
        this.StartUpLogger = new StartUpLogger();
        this.WindowManager = new RhinoWindowManager();
    }

    /// <summary>
    /// Uses assembly resolver to load the Rhino assembly once per app domain.
    /// </summary>
    static RhinoCoreExtension()
    {
        Instance = new RhinoCoreExtension();
        _rhinoInstallDirectoryExists = Directory.Exists(_systemDir);
        if (_rhinoInstallDirectoryExists)
        {

#if DEBUGNET8 || RELEASENET8

#if DEBUGNET8
            // DEBUG ONLY: Registered before the resolvers below so it observes every request.
            ZooLicenseDiagnostics.Install();
#endif
            RegisterAssemblyResolver(_rhinoCommonAssemblyName, Path.Combine(_systemDir, "netcore", _rhinoCommonDllName));
            RegisterAssemblyResolver("Rhino.UI", Path.Combine(_systemDir, "netcore", "Rhino.UI.dll"));
            RegisterAssemblyResolver("Mono.Cecil", Path.Combine(_systemDir, "netcore", "Mono.Cecil.dll"));

            LoadWcfAssemblies();
#else
            RegisterAssemblyResolver(_rhinoCommonAssemblyName, Path.Combine(_systemDir, _rhinoCommonDllName));

#endif

            RegisterAssemblyResolver(_grasshopperAssemblyName, Path.Combine(_pluginDir, _grasshopperDllRelativePath));

            RegisterAssemblyResolver(_grasshopperIoAssemblyName, Path.Combine(_pluginDir, _grasshopperIoDllRelativePath));
            RegisterAssemblyResolver("Eto", Path.Combine(_systemDir, "Eto.dll"));

        }
        else
        {
            Instance.StartUpLogger.AddError(_rhinoNotInstalledErrorMessage);
        }
    }

    /// <summary>
    /// Preloads the WCF client assemblies that ZooClient (LAN Zoo licensing) needs.
    /// WCF is not part of the .NET 8 runtime, and the host loads its own
    /// System.ServiceModel.Primitives at startup (6.0 in AutoCAD 2025, 8.1 in
    /// AutoCAD 2026), so every WCF type must unify on the host's version family:
    /// the System.ServiceModel facade (type forwards only) and the HTTP transport
    /// ship with this plugin, the latter version-matched to the host's Primitives.
    /// These must be PRELOADED rather than registered with AssemblyResolve: an
    /// already-loaded assembly wins every resolution path, whereas resolver events
    /// run after Rhino's own in-process prober, which would serve its 4.9 family
    /// from System\netcore. Mixing that family with the host's Primitives splits
    /// the WCF type identities and channel creation fails with "lacks a
    /// TransportBindingElement".
    /// </summary>
    private static void LoadWcfAssemblies()
    {
        try
        {
            var pluginDirectory =
                Path.GetDirectoryName(typeof(RhinoCoreExtension).Assembly.Location) ??
                string.Empty;

            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

            var hostDirectory = Path.GetDirectoryName(exePath);

            var hostPrimitives =
              Path.Combine(hostDirectory, "System.ServiceModel.Primitives.dll");

            var assemblyVersion = AssemblyName.GetAssemblyName(hostPrimitives).Version;

            var hostPrimitivesExists = File.Exists(hostPrimitives);

            var family = (assemblyVersion, hostPrimitivesExists) switch
            {
                ({ Major: 6 }, true) => _serviceModelFamliy6_0,
                ({ Major: 8, Minor: 0 }, true) => _serviceModelFamliy8_0,
                _ => _serviceModelFamliy8_1
            };

            Assembly.LoadFrom(Path.Combine(pluginDirectory, "System.ServiceModel.dll"));
            Assembly.LoadFrom(Path.Combine(pluginDirectory,
                string.Format(_systemHttpDll, family)));

            // The host normally supplies Primitives itself; ship our own only when absent.
            if (!hostPrimitivesExists)
                Assembly.LoadFrom(Path.Combine(pluginDirectory,
                    string.Format(_systemPrimitiveDll, family)));
        }
        catch (Exception e)
        {
            Instance.StartUpLogger.AddError(string.Format(_wcfErrorMessage,
                e.Message));
        }
    }

    /// <summary>
    /// Registers an assembly resolver for the specified assembly name.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly to resolve.</param>
    /// <param name="assemblyPath">The path of the assembly to resolve.</param>
    private static void RegisterAssemblyResolver(string assemblyName, string assemblyPath)
    {
        ResolveEventHandler? resolver = null;

        AppDomain.CurrentDomain.AssemblyResolve += resolver = (_, args) =>
        {
            var requestedAssemblyName = new AssemblyName(args.Name).Name;

            if (requestedAssemblyName != assemblyName)
                return null;

            AppDomain.CurrentDomain.AssemblyResolve -= resolver;

            return Assembly.LoadFrom(assemblyPath);
        };
    }

    /// <summary>
    /// Disposes the Rhino core when the rhino window is closed.
    /// </summary>
    private void OnClosing(object sender, EventArgs e)
    {
        RhinoApp.Closing -= this.OnClosing;

        _rhinoCore?.Dispose();
    }

    /// <summary>
    /// Creates the Rhino core instance.
    /// </summary>
    private void CreateCore()
    {
        try
        {
            var schemeName = string.Format(
                _rhinoInsideSchemeNameFormat,
                HostApplicationServices.Current.Product,
                HostApplicationServices.Current.releaseMarketVersion);

            var style = WindowStyle.Hidden;

            var autocadHandle = Autodesk.AutoCAD.ApplicationServices.Core.Application
                .MainWindow.Handle;

            var args = new List<string>()
            {
               _rhinoNoSplashArgument,
                string.Format(_rhinoSchemeArgumentFormat, schemeName)
            };

#if DEBUGNET8 || RELEASENET8
            args.Add("/netcore");
#else
            args.Add("/netfx");
#endif

            _rhinoCore ??= new RhinoCore(args.ToArray(), style, autocadHandle);

            var mainWindow = RhinoApp.MainWindowHandle();

            this.WindowManager.SetWindow(mainWindow);

            // Install CBT hook to automatically show window when user input is needed.
            // The hook detects activation attempts and shows the window if RhinoGet.InGet() is true.
            this.WindowManager.InstallActivationHook();

            RhinoApp.Closing += this.OnClosing;

        }
        catch
        {
            this.StartUpLogger.AddError(_rhinoCoreInitializationFailedErrorMessage);

            throw;
        }
    }

    /// <summary>
    /// Ensures that the Rhino core is, created and running, if there is not an existing
    /// instance then it creates one.
    /// </summary>
    public void ValidateRhinoCore()
    {
        if (_rhinoCore == null)
            this.CreateCore();
    }

    /// <summary>
    /// Disposes all IDisposable objects held in a parameter's volatile data.
    /// </summary>
    private void DisposeParamData(IGH_Param param)
    {
        var dataCount = param.VolatileDataCount;
        if (dataCount > 0)
        {
            System.Diagnostics.Debug.WriteLine(
                $"    Param '{param.Name}' has {dataCount} data item(s)");
        }

        foreach (var data in param.VolatileData.AllData(true))
        {
            if (data == null) continue;

            // Try to get the underlying value from the Goo
            object? valueToDispose = null;

            if (data is IGH_Goo goo)
            {
                valueToDispose = goo.ScriptVariable();
            }

            if (valueToDispose is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Failed to dispose {valueToDispose.GetType().Name}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Handles the Grasshopper DocumentRemoved event.
    /// </summary>
    private void OnDocumentRemoved(object server, Grasshopper.Kernel.GH_Document document)
    {
        foreach (var ghObject in document.Objects)
        {
            if (ghObject is IGH_Component component)
            {
                foreach (var param in component.Params.Output)
                {
                    this.DisposeParamData(param);
                }
            }
            else if (ghObject is IGH_Param param)
            {
                this.DisposeParamData(param);
            }
        }
    }

    /// <summary>
    /// The steps to take to shut down this rhino inside extension.
    /// </summary>
    public void Shutdown()
    {
        System.Diagnostics.Debug.WriteLine("=== RhinoCoreExtension.Shutdown() START ===");

        RhinoApp.Closing -= this.OnClosing;

        try
        {
            this.WindowManager.BringToFront();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WindowManager.BringToFront failed: {ex.Message}");
        }

        // Clean up the window manager (uninstalls CBT hook)
        this.WindowManager.Dispose();

        try
        {
            Grasshopper.Instances.DocumentServer.DocumentRemoved += this.OnDocumentRemoved;

            _rhinoCore?.Dispose();

            Grasshopper.Instances.DocumentServer.DocumentRemoved -= this.OnDocumentRemoved;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RhinoCore disposal failed: {ex.Message}");
        }
        finally
        {
            _rhinoCore = null;
        }

        // Note: RhinoDoc.ActiveDoc and Grasshopper.Instances.ActiveDocument
        // are disposed by _rhinoCore.Dispose() - do NOT dispose them again

        System.Diagnostics.Debug.WriteLine("=== RhinoCoreExtension.Shutdown() END ===");
    }
}