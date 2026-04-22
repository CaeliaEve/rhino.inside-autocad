using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps boundary data extracted from a Civil 3D TIN Surface.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted boundary information.
/// Unlike other Civil 3D wrappers, this does not wrap a database object since
/// <c>SurfaceBoundary</c> is a definition within a TinSurface, not a standalone entity.
/// </remarks>
public class CivilSurfaceBoundary : ICivilSurfaceBoundary
{
    /// <inheritdoc />
    public CivilSurfaceBoundaryType BoundaryType { get; }

    /// <inheritdoc />
    public RhinoCurve Curve { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilSurfaceBoundary"/>.
    /// </summary>
    /// <param name="boundaryType">
    /// The boundary type (Outer, DataClip, Hide, or Show).
    /// </param>
    /// <param name="polyline">
    /// The boundary geometry as a Rhino polyline.
    /// </param>
    /// <param name="name">
    /// The name of the boundary definition.
    /// </param>
    public CivilSurfaceBoundary(CivilSurfaceBoundaryType boundaryType, RhinoCurve polyline, string name)
    {
        this.BoundaryType = boundaryType;
        this.Curve = polyline;
        this.Name = name ?? string.Empty;
    }

    /// <summary>
    /// Creates a duplicate of this boundary wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilSurfaceBoundary Duplicate()
    {

        return new CivilSurfaceBoundary(this.BoundaryType, this.Curve.DuplicateCurve(), this.Name);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Surface Boundary [{this.BoundaryType}] \"{this.Name}\"";
    }
}
