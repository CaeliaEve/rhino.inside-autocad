namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Decides which of the installed Rhino versions Rhino.Inside binds to for this session.
/// </summary>
/// <remarks>
/// The choice is made once per process, on the startup path, before any RhinoCommon type is
/// touched - the assembly resolvers which serve Rhino's assemblies bake in the paths of the
/// chosen installation and cannot be re-pointed afterwards. Implementations therefore must
/// not reference any Rhino type themselves.
/// </remarks>
/// <seealso cref="IRhinoInstallation"/>
/// <seealso cref="IRhinoInstallationLocator"/>
public interface IRhinoVersionSelection
{
    /// <summary>
    /// Determines the Rhino installation to bind this session to, asking the user when the
    /// machine has more than one and they have not already settled on a version.
    /// </summary>
    /// <param name="anySupportedVersionInstalled">
    /// True if this machine has at least one Rhino version this build can host. Tells a null
    /// return caused by the user cancelling apart from one caused by there being nothing to
    /// choose from, which need different messages.
    /// </param>
    /// <returns>
    /// The installation to bind to, or null when no supported version is installed or the
    /// user declined to choose one.
    /// </returns>
    IRhinoInstallation? Resolve(out bool anySupportedVersionInstalled);
}
