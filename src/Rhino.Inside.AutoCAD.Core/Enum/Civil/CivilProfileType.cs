namespace Rhino.Inside.AutoCAD.Core;

/// <summary>
/// Represents the type of a Civil 3D Profile.
/// </summary>
public enum CivilProfileType
{
    /// <summary>
    /// A profile representing existing ground conditions, typically sampled from a surface.
    /// </summary>
    ExistingGround = 0,

    /// <summary>
    /// A layout profile that is manually designed or edited.
    /// </summary>
    Layout = 1,

    /// <summary>
    /// A profile that is superimposed from another source.
    /// </summary>
    SuperimposedProfile = 2,

    /// <summary>
    /// A quick profile created from selected objects.
    /// </summary>
    Quick = 3
}
