namespace Rhino.Inside.AutoCAD.Services;

/// <summary>
/// Contains application-wide constant values for the Rhino.Inside.AutoCAD plugin.
/// </summary>
/// <remarks>
/// This class centralizes all string literals, paths, icon URIs, registry keys, and
/// message templates used throughout the application. Constants are grouped by functionality:
/// assembly paths, framework detection, UI elements, Rhino integration, and error messages.
/// </remarks>
public class ApplicationConstants
{
    /// <summary>
    /// The folder name containing platform-specific application assemblies.
    /// </summary>
    /// <remarks>
    /// Value: "Win64". Located under the installation bundle directory.
    /// </remarks>
    public const string AssemblyFolderName = "Win64";

    /// <summary>
    /// Filter string used to detect .NET Framework runtime.
    /// </summary>
    /// <remarks>
    /// Returns ".NET Framework" for .NET Framework 4.8, or ".NET" for .NET 8+.
    /// Used during bootstrap to determine the correct assembly loading strategy.
    /// </remarks>
    /// <seealso cref="Net48FolderName"/>
    /// <seealso cref="Net8FolderName"/>
    public const string NetFrameworkFilter = ".NET Framework";

    /// <summary>
    /// Folder name for .NET Framework 4.8 assemblies.
    /// </summary>
    /// <remarks>
    /// Value: "NET48". Used when running under AutoCAD versions targeting .NET Framework.
    /// </remarks>
    /// <seealso cref="Net8FolderName"/>
    public const string Net48FolderName = "NET48";

    /// <summary>
    /// Folder name for .NET 8 assemblies.
    /// </summary>
    /// <remarks>
    /// Value: "NET8". Used when running under AutoCAD versions targeting .NET 8.
    /// </remarks>
    /// <seealso cref="Net48FolderName"/>
    public const string Net8FolderName = "NET8";

    /// <summary>
    /// Folder name for embedded application resources.
    /// </summary>
    public const string ResourcesFolderName = "Resources";

    /// <summary>
    /// Filename of the core settings JSON configuration file.
    /// </summary>
    /// <remarks>
    /// Value: "SettingsCore.json". Located in the <see cref="ResourcesFolderName"/> folder.
    /// </remarks>
    public const string SettingJsonName = "SettingsCore.json";

    /// <summary>
    /// Assembly filenames for Material Design WPF dependencies.
    /// </summary>
    /// <remarks>
    /// These assemblies must be loaded for the WPF UI components to render correctly.
    /// Includes MaterialDesignThemes.Wpf.dll, MaterialDesignColors.dll, and Microsoft.Xaml.Behaviors.dll.
    /// </remarks>
    public static List<string> MaterialDesignAssemblyNames =
    [
        "MaterialDesignThemes.Wpf.dll",
        "MaterialDesignColors.dll",
        "Microsoft.Xaml.Behaviors.dll"
    ];

    /// <summary>
    /// Filename of the Serilog logging configuration file.
    /// </summary>
    /// <remarks>
    /// Value: "SerilogConfig.json". Located in the <see cref="ResourcesFolderName"/> folder.
    /// </remarks>
    public const string LogConfigName = "SerilogConfig.json";

    /// <summary>
    /// Product name as registered in the assembly metadata.
    /// </summary>
    /// <remarks>
    /// Value: "Rhino.Inside.AutoCAD". Must match the Product name in Directory.Build.props.
    /// </remarks>
    public const string ProductName = "Rhino.Inside.AutoCAD";

    /// <summary>
    /// Application identifier used for internal references and folder names.
    /// </summary>
    /// <remarks>
    /// Value: "RhinoInsideAutoCAD". A compact form without dots for filesystem compatibility.
    /// </remarks>
    public const string ApplicationName = "RhinoInsideAutoCAD";

    /// <summary>
    /// Relative path from ProgramData to the application bundle installation folder.
    /// </summary>
    /// <remarks>
    /// Value: "Autodesk\ApplicationPlugins\Rhino.Inside.AutoCAD.bundle".
    /// This is the standard AutoCAD plugin installation location.
    /// </remarks>
    public const string RootInstallFolderName =
        "Autodesk\\ApplicationPlugins\\Rhino.Inside.AutoCAD.bundle";

    /// <summary>
    /// Relative path from AppData to the user-specific application data folder.
    /// </summary>
    /// <remarks>
    /// Value: "Bimorph\RhinoInsideAutoCAD". Stores user settings, logs, and cached data.
    /// </remarks>
    public const string ApplicationFolderName = "Bimorph\\RhinoInsideAutoCAD";

    /// <summary>
    /// Prefix used for versioned deployment package names.
    /// </summary>
    /// <remarks>
    /// Value: "RhinoInsideAutoCAD.Applications.". Combined with version number for package naming.
    /// </remarks>
    public const string PackagePrefixName = "RhinoInsideAutoCAD.Applications.";

    /// <summary>
    /// Display name for the Rhino.Inside tab in the AutoCAD ribbon.
    /// </summary>
    public const string RhinoInsideTabName = "Rhino.Inside";

    /// <summary>
    /// Pixel dimension for small ribbon icons (16x16).
    /// </summary>
    /// <seealso cref="LargeIconSize"/>
    public const int SmallIconSize = 16;

    /// <summary>
    /// Pixel dimension for large ribbon icons (32x32).
    /// </summary>
    /// <seealso cref="SmallIconSize"/>
    public const int LargeIconSize = 32;

    /// <summary>
    /// Unique identifier for the Grasshopper preview "Off" button.
    /// </summary>
    /// <seealso cref="ShadedButtonId"/>
    /// <seealso cref="WireframeButtonId"/>
    public const string OffButtonId = "GrasshopperPreviewOffButtonId";

    /// <summary>
    /// Unique identifier for the Grasshopper preview "Shaded" button.
    /// </summary>
    /// <seealso cref="OffButtonId"/>
    /// <seealso cref="WireframeButtonId"/>
    public const string ShadedButtonId = "GrasshopperPreviewShadedButtonId";

    /// <summary>
    /// Unique identifier for the Grasshopper preview "Wireframe" button.
    /// </summary>
    /// <seealso cref="OffButtonId"/>
    /// <seealso cref="ShadedButtonId"/>
    public const string WireframeButtonId = "GrasshopperPreviewWireframeButtonId";

    /// <summary>
    /// Pack URI for the unselected "Off" preview button icon.
    /// </summary>
    /// <seealso cref="OffButtonSelected"/>
    public const string OffButtonUnselected =
        "pack://application:,,,/Rhino.Inside.AutoCAD.Applications;component/Icons/Large512/Grasshopper_Preview_Off.png";

    /// <summary>
    /// Pack URI for the selected "Off" preview button icon.
    /// </summary>
    /// <seealso cref="OffButtonUnselected"/>
    public const string OffButtonSelected =
        "pack://application:,,,/Rhino.Inside.AutoCAD.Applications;component/Icons/Large512/Grasshopper_Preview_Off_Selected.png";

    /// <summary>
    /// Pack URI for the unselected "Shaded" preview button icon.
    /// </summary>
    /// <seealso cref="ShadedButtonSelected"/>
    public const string ShadedButtonUnselected =
        "pack://application:,,,/Rhino.Inside.AutoCAD.Applications;component/Icons/Large512/Grasshopper_Preview_Shaded.png";

    /// <summary>
    /// Pack URI for the selected "Shaded" preview button icon.
    /// </summary>
    /// <seealso cref="ShadedButtonUnselected"/>
    public const string ShadedButtonSelected =
        "pack://application:,,,/Rhino.Inside.AutoCAD.Applications;component/Icons/Large512/Grasshopper_Preview_Shaded_Selected.png";

    /// <summary>
    /// Pack URI for the unselected "Wireframe" preview button icon.
    /// </summary>
    /// <seealso cref="WireframeButtonSelected"/>
    public const string WireframeButtonUnselected =
        "pack://application:,,,/Rhino.Inside.AutoCAD.Applications;component/Icons/Large512/Grasshopper_Preview_Wireframe.png";

    /// <summary>
    /// Pack URI for the selected "Wireframe" preview button icon.
    /// </summary>
    /// <seealso cref="WireframeButtonUnselected"/>
    public const string WireframeButtonSelected =
        "pack://application:,,,/Rhino.Inside.AutoCAD.Applications;component/Icons/Large512/Grasshopper_Preview_Wireframe_Selected.png";

    /// <summary>
    /// Windows Registry key path for Rhino 8 installation information.
    /// </summary>
    /// <remarks>
    /// Used to locate the Rhino installation directory and plugins folder.
    /// </remarks>
    /// <seealso cref="RhinoInstallPathValueName"/>
    /// <seealso cref="RhinoPluginsFolderValueName"/>
    public const string RhinoRegistryKeyPath =
        @"HKEY_LOCAL_MACHINE\SOFTWARE\McNeel\Rhinoceros\8.0\Install";

    /// <summary>
    /// Registry value name for the Rhino installation directory path.
    /// </summary>
    /// <seealso cref="RhinoRegistryKeyPath"/>
    public const string RhinoInstallPathValueName = "Path";

    /// <summary>
    /// Registry value name for the default Rhino plugins folder path.
    /// </summary>
    /// <seealso cref="RhinoRegistryKeyPath"/>
    public const string RhinoPluginsFolderValueName = "Default Plug-ins Folder";

    /// <summary>
    /// Assembly name for RhinoCommon (without file extension).
    /// </summary>
    /// <remarks>
    /// Used for assembly resolution and binding redirects.
    /// </remarks>
    /// <seealso cref="RhinoCommonDllName"/>
    public const string RhinoCommonAssemblyName = "RhinoCommon";

    /// <summary>
    /// Assembly name for Grasshopper (without file extension).
    /// </summary>
    /// <seealso cref="GrasshopperDllRelativePath"/>
    public const string GrasshopperAssemblyName = "Grasshopper";

    /// <summary>
    /// Assembly name for GH_IO (without file extension).
    /// </summary>
    /// <seealso cref="GrasshopperIoDllRelativePath"/>
    public const string GrasshopperIOAssemblyName = "GH_IO";

    /// <summary>
    /// Filename for the RhinoCommon assembly.
    /// </summary>
    /// <seealso cref="RhinoCommonAssemblyName"/>
    public const string RhinoCommonDllName = "RhinoCommon.dll";

    /// <summary>
    /// Relative path from the plugins folder to Grasshopper.dll.
    /// </summary>
    /// <seealso cref="GrasshopperAssemblyName"/>
    public const string GrasshopperDllRelativePath = "Grasshopper//Grasshopper.dll";

    /// <summary>
    /// Relative path from the plugins folder to GH_IO.dll.
    /// </summary>
    /// <seealso cref="GrasshopperIOAssemblyName"/>
    public const string GrasshopperIoDllRelativePath = "Grasshopper//GH_IO.dll";

    /// <summary>
    /// Command-line argument to suppress the Rhino splash screen on startup.
    /// </summary>
    /// <remarks>
    /// Value: "/nosplash". Used when launching Rhino in embedded mode.
    /// </remarks>
    public const string RhinoNoSplashArgument = "/nosplash";

    /// <summary>
    /// Format string for specifying a Rhino scheme via command-line.
    /// </summary>
    /// <remarks>
    /// Value: "/scheme={0}". The placeholder receives the scheme name.
    /// </remarks>
    /// <seealso cref="RhinoInsideSchemeNameFormat"/>
    public const string RhinoSchemeArgumentFormat = "/scheme={0}";

    /// <summary>
    /// Format string for generating the Rhino.Inside scheme name.
    /// </summary>
    /// <remarks>
    /// Value: "Inside-{0}-{1}". Placeholders typically receive host application name and version.
    /// </remarks>
    /// <seealso cref="RhinoSchemeArgumentFormat"/>
    public const string RhinoInsideSchemeNameFormat = "Inside-{0}-{1}";

    /// <summary>
    /// Error message displayed when Rhino 8 is not detected on the system.
    /// </summary>
    /// <seealso cref="RhinoRegistryKeyPath"/>
    public const string RhinoNotInstalledErrorMessage =
        "Rhino 8 not installed or could not be found. The application requires Rhino 8 to run.";

    /// <summary>
    /// Error message displayed when RhinoCore initialization fails.
    /// </summary>
    public const string RhinoCoreInitializationFailedErrorMessage =
        "Failed to initialize Rhino Core";

    /// <summary>
    /// Success message written to the AutoCAD command line after successful initialization.
    /// </summary>
    public const string ApplicationLoadedSuccessMessage =
        "\nRhino.Inside.AutoCAD loaded successfully.";

    /// <summary>
    /// Format string for error messages during application loading.
    /// </summary>
    /// <remarks>
    /// Value: "\nERROR loading Rhino.Inside.AutoCAD: {0}\n". Placeholder receives exception message.
    /// </remarks>
    /// <seealso cref="StackTraceMessageFormat"/>
    public const string ApplicationLoadErrorMessageFormat =
        "\nERROR loading Rhino.Inside.AutoCAD: {0}\n";

    /// <summary>
    /// Format string for displaying exception stack traces.
    /// </summary>
    /// <remarks>
    /// Value: "\nStack trace: {0}\n". Placeholder receives the stack trace string.
    /// </remarks>
    /// <seealso cref="ApplicationLoadErrorMessageFormat"/>
    public const string StackTraceMessageFormat = "\nStack trace: {0}\n";

    /// <summary>
    /// Message displayed when the application license has expired.
    /// </summary>
    /// <remarks>
    /// Includes a URL directing users to download an updated version.
    /// </remarks>
    public const string ExpiredMessage =
        "This version of Rhino.Inside.AutoCAD has expired. Please download the latest version.";

    /// <summary>
    /// URL to download the latest version of Rhino.Inside.AutoCAD.
    /// </summary>
    public const string DownloadUrl = "https://www.bimorph.com/products/rhino-inside-autocad";

    /// <summary>
    /// Error message displayed when the RhinoDoc fails to initialize.
    /// </summary>
    public const string FailedToLoadRhinoDoc = "Failed to initialize Rhino Doc.";

    /// <summary>
    /// Prefix used to denote build metadata in semantic version strings.
    /// </summary>
    /// <remarks>
    /// Value: "+build". Follows SemVer 2.0 specification for build metadata.
    /// </remarks>
    public const string BuildVersionMetadataPrefix = "+build";

    /// <summary>
    /// Unique identifier for the Rhino object preview toggle button.
    /// </summary>
    /// <seealso cref="RhinocerosPreviewShadedIcon"/>
    /// <seealso cref="RhinocerosPreviewOffIcon"/>
    public const string RhinoPreviewButtonId = "RhinoPreviewButtonId";

    /// <summary>
    /// Unique identifier for the Grasshopper solver toggle button.
    /// </summary>
    /// <seealso cref="GrasshopperSolverOnIcon"/>
    /// <seealso cref="GrasshopperSolverOffIcon"/>
    public const string GrasshopperSolverButtonId = "GrasshopperSolverButtonId";

    /// <summary>
    /// Pack URI for the Rhino preview shaded (enabled) icon.
    /// </summary>
    /// <seealso cref="RhinocerosPreviewOffIcon"/>
    public const string RhinocerosPreviewShadedIcon =
        "pack://application:,,,/Rhino.Inside.AutoCAD.Applications;component/Icons/Large512/Rhinoceros_Preview_Shaded.png";

    /// <summary>
    /// Pack URI for the Rhino preview off (disabled) icon.
    /// </summary>
    /// <seealso cref="RhinocerosPreviewShadedIcon"/>
    public const string RhinocerosPreviewOffIcon =
        "pack://application:,,,/Rhino.Inside.AutoCAD.Applications;component/Icons/Large512/Rhinoceros_Preview_Off.png";

    /// <summary>
    /// Pack URI for the Grasshopper solver enabled icon.
    /// </summary>
    /// <seealso cref="GrasshopperSolverOffIcon"/>
    public const string GrasshopperSolverOnIcon =
        "pack://application:,,,/Rhino.Inside.AutoCAD.Applications;component/Icons/Large512/Grasshopper_SolverOn.png";

    /// <summary>
    /// Pack URI for the Grasshopper solver disabled icon.
    /// </summary>
    /// <seealso cref="GrasshopperSolverOnIcon"/>
    public const string GrasshopperSolverOffIcon =
        "pack://application:,,,/Rhino.Inside.AutoCAD.Applications;component/Icons/Large512/Grasshopper_SolverOff.png";

    /// <summary>
    /// Rhino command name that launches the Grasshopper editor.
    /// </summary>
    public const string GrasshopperCommandName = "Grasshopper";

    /// <summary>
    /// Rhino command name that opens the Package Manager.
    /// </summary>
    public const string PackageManagerCommandName = "PackageManager";

    /// <summary>
    /// Rhino command name that launches Grasshopper Player.
    /// </summary>
    public const string GrasshopperPlayerCommandName = "GrasshopperPlayer";

    /// <summary>
    /// Rhino script macro for creating a new floating viewport with copied projection.
    /// </summary>
    /// <remarks>
    /// Value: "_NewFloatingViewport _Projection _CopyActive".
    /// </remarks>
    public const string NewFloatingViewportScript =
        "_NewFloatingViewport _Projection _CopyActive";

    /// <summary>
    /// Format string for the default Rhino template file path.
    /// </summary>
    /// <remarks>
    /// Value: "{0}Large Objects - Millimeters.3dm". Placeholder receives the templates directory path.
    /// </remarks>
    public const string DefaultTemplateFormat = "{0}Large Objects - Millimeters.3dm";
}
