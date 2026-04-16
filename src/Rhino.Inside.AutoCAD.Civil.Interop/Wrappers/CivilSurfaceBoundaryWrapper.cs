using Rhino.Geometry;
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
public class CivilSurfaceBoundaryWrapper : ICivilSurfaceBoundary
{
    /// <inheritdoc />
    public int BoundaryType { get; }

    /// <inheritdoc />
    public Polyline Polyline { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilSurfaceBoundaryWrapper"/>.
    /// </summary>
    /// <param name="boundaryType">
    /// The boundary type (0=Outer, 1=DataClip, 2=Hide, 3=Show).
    /// </param>
    /// <param name="polyline">
    /// The boundary geometry as a Rhino polyline.
    /// </param>
    /// <param name="name">
    /// The name of the boundary definition.
    /// </param>
    public CivilSurfaceBoundaryWrapper(int boundaryType, Polyline polyline, string name)
    {
        BoundaryType = boundaryType;
        Polyline = polyline;
        Name = name ?? string.Empty;
    }

    /// <summary>
    /// Creates a duplicate of this boundary wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilSurfaceBoundaryWrapper Duplicate()
    {
        var polylineCopy = new Polyline(Polyline);
        return new CivilSurfaceBoundaryWrapper(BoundaryType, polylineCopy, Name);
    }

    /// <summary>
    /// Gets a human-readable description of the boundary type.
    /// </summary>
    public string BoundaryTypeName => BoundaryType switch
    {
        0 => "Outer",
        1 => "DataClip",
        2 => "Hide",
        3 => "Show",
        _ => "Unknown"
    };

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Surface Boundary [{BoundaryTypeName}] \"{Name}\"";
    }
}
