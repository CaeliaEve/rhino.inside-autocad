using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a point on a Civil 3D surface with X, Y coordinates and elevation.
/// </summary>
public interface ICivilSurfacePoint
{
    /// <summary>
    /// Gets the X coordinate of the surface point.
    /// </summary>
    double X { get; }

    /// <summary>
    /// Gets the Y coordinate of the surface point.
    /// </summary>
    double Y { get; }

    /// <summary>
    /// Gets the elevation (Z coordinate) of the surface point.
    /// </summary>
    double Elevation { get; }

    /// <summary>
    /// Converts this surface point to a Rhino Point3d.
    /// </summary>
    /// <returns>A Point3d with X, Y, and Elevation as Z.</returns>
    Point3d ToRhinoPoint3d();
}
