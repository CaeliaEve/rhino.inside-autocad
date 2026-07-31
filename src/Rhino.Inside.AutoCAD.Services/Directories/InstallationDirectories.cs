using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Services;

/// <inheritdoc cref="IInstallationDirectories"/>
public class InstallationDirectories : IInstallationDirectories
{
    private const string _assemblyFolder = ApplicationConstants.AssemblyFolderName;
    private const string _resourcesFolderName = ApplicationConstants.ResourcesFolderName;
    private const string _net48FolderName = ApplicationConstants.Net48FolderName;
    private const string _net8FolderName = ApplicationConstants.Net8FolderName;
    private const string _net10FolderName = ApplicationConstants.Net10FolderName;

    /// <inheritdoc />
    public string RootInstallationLocation { get; }

    /// <inheritdoc />
    public string Resources { get; }

    /// <inheritdoc />
    public string VersionedAssemblies { get; }

    /// <inheritdoc />
    public string ApplicationName { get; }

    /// <inheritdoc />
    public string ProductName { get; }

    /// <summary>
    /// Constructs a new <see cref="IInstallationDirectories"/>.
    /// </summary>
    public InstallationDirectories(IApplicationVersionHistory versionHistory, IApplicationConfig applicationConfig)
    {
        var frameworkFolder = this.GetFrameworkFolder();

        var currentVersion = versionHistory.GetCurrentVersion();

        var rootInstallDirectory = applicationConfig.RootInstallDirectory;

        var applicationName = applicationConfig.ApplicationName;

        this.RootInstallationLocation = rootInstallDirectory;

        this.VersionedAssemblies = Path.Combine(rootInstallDirectory, currentVersion.ToString(), _assemblyFolder, frameworkFolder);

        this.Resources = Path.Combine(rootInstallDirectory, _resourcesFolderName);

        this.ApplicationName = applicationName;

        this.ProductName = applicationConfig.ProductName;
    }

    /// <summary>
    /// Returns the folder name of the deployed leg these assemblies were built for:
    /// "NET48", "NET8" or "NET10".
    /// </summary>
    /// <remarks>
    /// Decided at compile time rather than from <c>RuntimeInformation.FrameworkDescription</c>,
    /// because the runtime cannot tell the .NET legs apart: the NET8 leg is built to run
    /// under .NET 10 as well, serving the 2025/2026 releases Autodesk moved to .NET 10
    /// without changing their series. Reading the runtime instead sent the NET10 leg to the
    /// NET8 folder, and since AutoCAD had already loaded the component libraries from NET10,
    /// <see cref="System.Reflection.Assembly.LoadFrom(string)"/> refused the second copy and
    /// the Grasshopper components never registered.
    /// </remarks>
    public string GetFrameworkFolder()
    {
#if NET10_0_OR_GREATER
        return _net10FolderName;
#elif NET8_0_OR_GREATER
        return _net8FolderName;
#else
        return _net48FolderName;
#endif
    }
}