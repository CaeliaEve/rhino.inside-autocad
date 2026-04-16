using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a boundary definition from a Civil 3D TIN Surface.
/// </summary>
/// <remarks>
/// Surface boundaries in Civil 3D define the outer limits, data clips, hide boundaries,
/// and show boundaries of a TIN surface. This interface provides access to the boundary's
/// geometry and type information.
/// <para>
/// Boundary types are represented as integers:
/// <list type="bullet">
/// <item><description>0 = Outer boundary</description></item>
/// <item><description>1 = DataClip boundary</description></item>
/// <item><description>2 = Hide boundary</description></item>
/// <item><description>3 = Show boundary</description></item>
/// </list>
/// </para>
/// </remarks>
public interface ICivilSurfaceBoundary
{
    /// <summary>
    /// Gets the boundary type as an integer.
    /// Maps to the Civil 3D SurfaceBoundaryType enumeration.
    /// </summary>
    /// <value>
    /// 0 = Outer, 1 = DataClip, 2 = Hide, 3 = Show
    /// </value>
    int BoundaryType { get; }

    /// <summary>
    /// Gets the boundary geometry as a Rhino polyline.
    /// </summary>
    Polyline Polyline { get; }

    /// <summary>
    /// Gets the name of the boundary definition.
    /// </summary>
    string Name { get; }
}
