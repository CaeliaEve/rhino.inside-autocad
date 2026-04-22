using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a boundary definition from a Civil 3D TIN Surface.
/// </summary>
/// <remarks>
/// Surface boundaries in Civil 3D define the outer limits, data clips, hide boundaries,
/// and show boundaries of a TIN surface. This interface provides access to the boundary's
/// geometry and type information.
/// </remarks>
public interface ICivilSurfaceBoundary
{
    /// <summary>
    /// Gets the boundary type (Outer, DataClip, Hide, or Show).
    /// </summary>
    CivilSurfaceBoundaryType BoundaryType { get; }

    /// <summary>
    /// Gets the boundary geometry as a Rhino polyline.
    /// </summary>
    RhinoCurve Curve { get; }

    /// <summary>
    /// Gets the name of the boundary definition.
    /// </summary>
    string Name { get; }
}
