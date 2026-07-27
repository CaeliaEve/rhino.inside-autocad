namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// The outcome of showing the Rhino version selection dialog.
/// </summary>
/// <seealso cref="IRhinoVersionDialogManager"/>
public interface IRhinoVersionDialogResult
{
    /// <summary>
    /// The choice the user made.
    /// </summary>
    RhinoVersionChoice Choice { get; }

    /// <summary>
    /// The installation the user selected, or null when they cancelled.
    /// </summary>
    IRhinoInstallation? Installation { get; }
}
