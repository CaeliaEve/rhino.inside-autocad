namespace Rhino.Inside.AutoCAD.Core;

/// <summary>
/// The outcome of the Rhino version selection dialog.
/// </summary>
public enum RhinoVersionChoice
{
    /// <summary>
    /// The user cancelled without choosing a version. Rhino is not loaded for the remainder
    /// of the session.
    /// </summary>
    Cancel = 0,

    /// <summary>
    /// The user chose a version for this session only, and wants to be asked again the next
    /// time AutoCAD starts.
    /// </summary>
    Use = 1,

    /// <summary>
    /// The user chose a version and does not want to be asked again.
    /// </summary>
    AlwaysUse = 2
}
