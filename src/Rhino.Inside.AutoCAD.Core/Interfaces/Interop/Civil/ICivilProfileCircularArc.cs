using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a circular arc entity from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// Profile circular arcs are curved vertical segments with a constant radius.
/// </remarks>
public interface ICivilProfileCircularArc : ICivilProfileEntity
{
    /// <summary>
    /// Gets the radius of this circular arc.
    /// </summary>
    double Radius { get; }

    /// <summary>
    /// Gets the center point of this circular arc.
    /// </summary>
    /// <remarks>
    /// The point is in 2D station-elevation space where X = Station and Y = Elevation.
    /// </remarks>
    Point3d CenterPoint { get; }

    /// <summary>
    /// Gets the geometry of this circular arc as a Rhino arc.
    /// </summary>
    /// <remarks>
    /// The arc is in 2D station-elevation space where X = Station and Y = Elevation.
    /// </remarks>
    Arc Arc { get; }
}
