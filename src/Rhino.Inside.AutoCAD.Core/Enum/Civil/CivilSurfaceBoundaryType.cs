namespace Rhino.Inside.AutoCAD.Core;

/// <summary>
/// Represents the type of boundary on a Civil 3D TIN Surface.
/// </summary>
public enum CivilSurfaceBoundaryType
{
    /// <summary>
    /// Outer boundary defining the surface extent.
    /// </summary>
    Outer = 0,

    /// <summary>
    /// Data clip boundary that limits the surface data area.
    /// </summary>
    DataClip = 1,

    /// <summary>
    /// Hide boundary that hides a portion of the surface.
    /// </summary>
    Hide = 2,

    /// <summary>
    /// Show boundary that reveals a portion of the surface.
    /// </summary>
    Show = 3
}
