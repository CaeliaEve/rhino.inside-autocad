using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a parabola (vertical curve) entity from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// Profile parabolas are the most common type of vertical curve, used for
/// smooth transitions between different grades. They are characterized by
/// their K value (rate of change of grade).
/// </remarks>
public interface ICivilProfileParabola : ICivilProfileEntity
{
    /// <summary>
    /// Gets the K value (rate of change of grade) of this parabola.
    /// </summary>
    /// <remarks>
    /// K value is the horizontal distance required to achieve a 1% change in grade.
    /// Higher K values indicate gentler curves.
    /// </remarks>
    double KValue { get; }

    /// <summary>
    /// Gets the station of the PVI (Point of Vertical Intersection).
    /// </summary>
    double PVIStation { get; }

    /// <summary>
    /// Gets the elevation of the PVI (Point of Vertical Intersection).
    /// </summary>
    double PVIElevation { get; }

    /// <summary>
    /// Gets the high or low point of this parabola, if one exists within the curve.
    /// </summary>
    /// <remarks>
    /// The point is in 2D station-elevation space where X = Station and Y = Elevation.
    /// Returns null if no high/low point exists within the curve limits.
    /// </remarks>
    Point3d? HighLowPoint { get; }
}
