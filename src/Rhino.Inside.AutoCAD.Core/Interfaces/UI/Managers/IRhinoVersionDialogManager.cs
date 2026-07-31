namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Manages the dialog which asks the user which installed Rhino version to run.
/// </summary>
/// <remarks>
/// Shown during AutoCAD's startup, before Rhino.Inside binds to an installation, so the
/// dialog blocks its caller until the user answers rather than being shown and forgotten
/// like the other dialogs.
/// </remarks>
/// <seealso cref="IRhinoVersionSelection"/>
public interface IRhinoVersionDialogManager
{
    /// <summary>
    /// Shows the dialog and blocks until the user answers.
    /// </summary>
    /// <param name="installations">
    /// The installations to choose between, ordered newest first. Must not be empty.
    /// </param>
    /// <param name="preselected">
    /// The installation to select initially, or null to select the first.
    /// </param>
    /// <returns>The user's choice.</returns>
    IRhinoVersionDialogResult Show(IReadOnlyList<IRhinoInstallation> installations,
        IRhinoInstallation? preselected = null);
}
