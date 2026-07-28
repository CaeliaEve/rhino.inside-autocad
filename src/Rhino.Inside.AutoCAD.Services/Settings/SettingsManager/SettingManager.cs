using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Services;

/// <inheritdoc cref="ISettingsManager"/>
public class SettingManager : ISettingsManager
{
    private readonly IInstallationDirectories _installationDirectories;

    /// <inheritdoc />
    public ISettings Core { get; }

    /// <inheritdoc />
    public IUserSettingsStore User { get; }

    /// <summary>
    /// Constructor for the <see cref="SettingManager"/>.
    /// </summary>
    public SettingManager(IInstallationDirectories installationDirectories)
    {
        _installationDirectories = installationDirectories;

        var coreSettingImporter = new SettingsImporter();

        this.Core = coreSettingImporter.Import(installationDirectories);

        // The same instance the startup path already read the Rhino version choice from,
        // so a change made on the settings page is seen without a reload.
        this.User = UserSettingsStore.Instance;
    }
}