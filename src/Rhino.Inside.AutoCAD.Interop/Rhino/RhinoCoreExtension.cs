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
    private const string _rhinoCommonAssemblyName = ApplicationConstants.RhinoCommonAssemblyName;
    private const string _grasshopperAssemblyName = ApplicationConstants.GrasshopperAssemblyName;
    private const string _grasshopperIoAssemblyName = ApplicationConstants.GrasshopperIOAssemblyName;
    private const string _rhinoCommonDllName = ApplicationConstants.RhinoCommonDllName;
    private const string _grasshopperDllRelativePath = ApplicationConstants.GrasshopperDllRelativePath;
    private const string _grasshopperIoDllRelativePath = ApplicationConstants.GrasshopperIoDllRelativePath;
    private const string _rhinoNoSplashArgument = ApplicationConstants.RhinoNoSplashArgument;
    private const string _rhinoSchemeArgumentFormat = ApplicationConstants.RhinoSchemeArgumentFormat;
    private const string _rhinoInsideSchemeNameFormat = ApplicationConstants.RhinoInsideSchemeNameFormat;
    private const string _rhinoCoreInitializationFailedErrorMessage = ApplicationConstants.RhinoCoreInitializationFailedErrorMessage;
    private const string _wcfErrorMessage = Services.MessageConstants.WcfErrorMessage;
    private const string _systemPrimitiveDll = ApplicationConstants.SystemPrimitiveDll;
    private const string _systemHttpDll = ApplicationConstants.SystemHttpDll;
    private const string _serviceModelFamliy8_0 = ApplicationConstants.ServiceModelFamliy8_0;
    private const string _serviceModelFamliy8_1 = ApplicationConstants.ServiceModelFamliy8_1;
    private const string _serviceModelFamliy6_0 = ApplicationConstants.ServiceModelFamliy6_0;

    private static RhinoCore? _rhinoCore;

    /// <summary>
    /// The <see cref="RhinoCoreExtension"/> singleton instance.
    /// </summary>
    public static RhinoCoreExtension Instance { get; }

    /// <summary>
    /// The Rhino installation this session is bound to.
    /// </summary>
    /// <remarks>
    /// Null only before <see cref="BindTo"/> has been called. The plugin does not finish
    /// loading unless the binding succeeds, so by the time any command can run this is set.
    /// </remarks>
    /// <seealso cref="BindTo"/>
    public static IRhinoInstallation? SelectedInstallation { get; private set; }

    /// <inheritdoc />
    public IStartUpLogger StartUpLogger { get; }

    /// <inheritdoc />
    public IRhinoWindowManager WindowManager { get; }

    /// <summary>
    /// Constructs a new <see cref="RhinoCoreExtension"/> instance.
    /// </summary>
    private RhinoCoreExtension()
    {
        this.StartUpLogger = new StartUpLogger();
        this.WindowManager = new RhinoWindowManager();
    }

    /// <summary>
    /// Creates the singleton. Deliberately does no work beyond that: the resolver
    /// registration below shows UI on the way to deciding which paths to register, and
    /// running that under the CLR's class initialization lock risks deadlock.
    /// </summary>
    static RhinoCoreExtension()
    {
        Instance = new RhinoCoreExtension();
    }

    /// <summary>
    /// Binds this app domain to a Rhino installation by registering the assembly resolvers
    /// which load its assemblies.
    /// </summary>
    /// <remarks>
    /// Must be called exactly once, before any RhinoCommon, Grasshopper or Eto type is
    /// touched. The resolvers bake in the paths of the given installation and cannot be
    /// re-pointed afterwards, which is why changing version needs an AutoCAD restart.
    /// </remarks>
    /// <param name="installation">The installation to bind to.</param>
    /// <seealso cref="RhinoVersionSelection.Resolve"/>
    public static void BindTo(IRhinoInstallation installation)
    {
        SelectedInstallation = installation;

#if DEBUG && NET8_0_OR_GREATER
        // DEBUG ONLY: Registered before the resolvers below so it observes every request.
        ZooLicenseDiagnostics.Install();
#endif

        var systemDir = installation.SystemDirectory;
        var pluginDir = installation.PluginsDirectory;

        RegisterAssemblyResolver(_rhinoCommonAssemblyName, installation.RhinoCommonPath);

#if NET8_0_OR_GREATER
        // Rhino.UI and Mono.Cecil sit alongside RhinoCommon in the netcore subfolder.
        var assemblyDir = installation.AssemblyDirectory;

        RegisterAssemblyResolver("Rhino.UI", Path.Combine(assemblyDir, "Rhino.UI.dll"));
        RegisterAssemblyResolver("Mono.Cecil", Path.Combine(assemblyDir, "Mono.Cecil.dll"));

        LoadWcfAssemblies();
#endif

        RegisterAssemblyResolver(_grasshopperAssemblyName, Path.Combine(pluginDir, _grasshopperDllRelativePath));

        RegisterAssemblyResolver(_grasshopperIoAssemblyName, Path.Combine(pluginDir, _grasshopperIoDllRelativePath));
        RegisterAssemblyResolver("Eto", Path.Combine(systemDir, "Eto.dll"));
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
    /// Optional host window handle for out-of-process embedding.
    /// </summary>
    public static IntPtr HostWindowHandle { get; set; } = IntPtr.Zero;

    /// <summary>
    /// Creates the Rhino core instance.
    /// </summary>
    private void CreateCore()
    {
        try
        {
            var style = WindowStyle.Hidden;

            var autocadHandle = HostWindowHandle;
            if (autocadHandle == IntPtr.Zero)
            {
                try
                {
                    autocadHandle = Autodesk.AutoCAD.ApplicationServices.Core.Application.MainWindow.Handle;
                }
                catch
                {
                    autocadHandle = IntPtr.Zero;
                }
            }

            var args = new List<string>()
            {
               _rhinoNoSplashArgument,
            };

#if NET8_0_OR_GREATER
            args.Add("/netcore");
#else
            if (SelectedInstallation?.MajorVersion >= 8)
            {
                args.Add("/netfx");
            }
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