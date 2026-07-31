using Microsoft.Win32;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Services;

/// <inheritdoc cref="IRhinoInstallationLocator"/>
/// <remarks>
/// Enumerates the version subkeys beneath
/// <see cref="ApplicationConstants.RhinoRegistryKeyPath"/> rather than reading a single
/// hard-coded version key, so a machine with more than one Rhino installed reports all of
/// them. Each key is handed to a <see cref="RhinoInstallation"/>, which reads itself from it
/// and decides whether this build can host it.
/// </remarks>
public class RhinoInstallationLocator : IRhinoInstallationLocator
{
    private const string _rhinoRegistryKeyPath = ApplicationConstants.RhinoRegistryKeyPath;
    private const string _rhinoInstallSubKeyName = ApplicationConstants.RhinoInstallSubKeyName;



    /// <summary>
    /// Reads a single version subkey, returning null when it holds no installation this
    /// build can host.
    /// </summary>
    /// <param name="rhinocerosKey">The parent Rhinoceros registry key.</param>
    /// <param name="versionKeyName">The version subkey name, for example "8.0".</param>
    private IRhinoInstallation? ReadInstallation(RegistryKey rhinocerosKey,
        string versionKeyName)
    {
        try
        {
            using var installKey = rhinocerosKey.OpenSubKey(
                $@"{versionKeyName}\{_rhinoInstallSubKeyName}");

            if (installKey == null)
                return null;

            var installation = new RhinoInstallation(versionKeyName, installKey);

            return installation.IsHostable ? installation : null;
        }
        catch (Exception e)
        {
            LoggerService.Instance.LogError(e);

            return null;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<IRhinoInstallation> Locate()
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                RegistryView.Registry64);

            using var rhinocerosKey = baseKey.OpenSubKey(_rhinoRegistryKeyPath);

            if (rhinocerosKey == null)
                return [];

            var installations = new List<IRhinoInstallation>();

            foreach (var versionKeyName in rhinocerosKey.GetSubKeyNames())
            {
                var installation = this.ReadInstallation(rhinocerosKey, versionKeyName);

                if (installation != null)
                    installations.Add(installation);
            }

            return installations
                .OrderByDescending(installation => installation.MajorVersion)
                .ThenByDescending(installation => installation.VersionKey,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception e)
        {
            LoggerService.Instance.LogError(e);

            return [];
        }
    }
}
