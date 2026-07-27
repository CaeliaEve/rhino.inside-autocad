namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Loads and persists the user-scoped <see cref="IUserSettings"/>.
/// </summary>
/// <remarks>
/// The store is read before the application is bootstrapped, so implementations must
/// resolve their own storage location and must never throw when the settings file is
/// missing or unreadable; defaults are returned instead.
/// </remarks>
/// <seealso cref="IUserSettings"/>
public interface IUserSettingsStore
{
    /// <summary>
    /// The current user settings.
    /// </summary>
    IUserSettings Settings { get; }

    /// <summary>
    /// Writes <see cref="Settings"/> to disk. Failures are swallowed and logged.
    /// </summary>
    void Save();

    /// <summary>
    /// Re-reads the settings from disk, discarding any unsaved changes.
    /// </summary>
    void Reload();
}
