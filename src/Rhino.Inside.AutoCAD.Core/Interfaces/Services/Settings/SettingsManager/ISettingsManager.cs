namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// The settings manager, This setting manager is used to import the <see
/// cref="ISettings"/>. The <see cref="ISettings"/> is the location
/// of the core settings which are common for all applications.
/// </summary>
public interface ISettingsManager
{
    /// <summary>
    /// The core settings shared by all applications.
    /// </summary>
    public ISettings Core { get; }

    /// <summary>
    /// The store for the user-scoped settings which persist between sessions.
    /// </summary>
    /// <seealso cref="IUserSettings"/>
    public IUserSettingsStore User { get; }
}