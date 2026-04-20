namespace Rhino.Inside.AutoCAD.Core;

/// <summary>
/// Represents the type of breakline on a Civil 3D TIN Surface.
/// </summary>
public enum SurfaceBreaklineType
{
    /// <summary>
    /// Standard breakline that defines surface edges.
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Wall breakline that creates vertical faces on the surface.
    /// </summary>
    Wall = 1,

    /// <summary>
    /// Non-destructive breakline that does not modify the original surface triangulation.
    /// </summary>
    NonDestructive = 2
}
