using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Represents a point on a Civil 3D surface with X, Y coordinates and elevation.
/// </summary>
public record CivilSurfacePoint : ICivilSurfacePoint
{
    /// <inheritdoc />
    public double X { get; }

    /// <inheritdoc />
    public double Y { get; }

    /// <inheritdoc />
    public double Elevation { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilSurfacePoint"/>.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="elevation">The elevation (Z coordinate).</param>
    public CivilSurfacePoint(double x, double y, double elevation)
    {
        this.X = x;
        this.Y = y;
        this.Elevation = elevation;
    }

    /// <inheritdoc />
    public Point3d ToRhinoPoint3d()
    {
        return new Point3d(this.X, this.Y, this.Elevation);
    }

    /// <summary>
    /// Returns a string representation of this surface point.
    /// </summary>
    public override string ToString()
    {
        return $"Surface Point ({this.X:F2}, {this.Y:F2}, {this.Elevation:F2})";
    }
}
