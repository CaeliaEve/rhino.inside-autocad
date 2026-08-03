namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Discovers the Rhino installations on this machine which this build is able to host.
/// </summary>
/// <seealso cref="IRhinoInstallation"/>
public interface IRhinoInstallationLocator
{
    /// <summary>
    /// Returns every hostable Rhino installation found, ordered newest version first.
    /// Returns an empty collection when none are found; never throws.
    /// </summary>
    IReadOnlyList<IRhinoInstallation> Locate();
}
