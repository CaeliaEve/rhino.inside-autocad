using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;

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
    public SurfaceBoundaryType BoundaryType { get; }

    /// <inheritdoc />
    public Polyline Polyline { get; }

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
    public CivilSurfaceBoundary(SurfaceBoundaryType boundaryType, Polyline polyline, string name)
    {
        this.BoundaryType = boundaryType;
        this.Polyline = polyline;
        this.Name = name ?? string.Empty;
    }

    /// <summary>
    /// Creates a duplicate of this boundary wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilSurfaceBoundary Duplicate()
    {
        var polylineCopy = new Polyline(this.Polyline);
        return new CivilSurfaceBoundary(this.BoundaryType, polylineCopy, this.Name);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Surface Boundary [{this.BoundaryType}] \"{this.Name}\"";
    }
}
