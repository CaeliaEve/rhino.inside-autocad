using Rhino.Inside.AutoCAD.Core.Interfaces;
using System.Runtime.InteropServices;

namespace Rhino.Inside.AutoCAD.Services;

/// <inheritdoc cref="IInstallationDirectories"/>
public class InstallationDirectories : IInstallationDirectories
{
    private const string _assemblyFolder = ApplicationConstants.AssemblyFolderName;
    private const string _resourcesFolderName = ApplicationConstants.ResourcesFolderName;
    private const string _netFrameworkFilter = ApplicationConstants.NetFrameworkFilter;
    private const string _net48FolderName = ApplicationConstants.Net48FolderName;
    private const string _net8FolderName = ApplicationConstants.Net8FolderName;

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
    /// Returns the framework folder name based on the current runtime framework.
    /// In case of .NET Framework 4.8, returns "NET48", otherwise "NET8".
    /// </summary>
    public string GetFrameworkFolder()
    {
        var description = RuntimeInformation.FrameworkDescription;

        return description.StartsWith(_netFrameworkFilter, StringComparison.OrdinalIgnoreCase)
            ? _net48FolderName
            : _net8FolderName;
    }
}