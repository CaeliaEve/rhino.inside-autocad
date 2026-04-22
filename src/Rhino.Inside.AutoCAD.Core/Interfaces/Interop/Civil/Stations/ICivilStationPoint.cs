using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a station value with its corresponding elevation.
/// </summary>
public interface ICivilStationPoint
{
    /// <summary>
    /// Gets the station value along an alignment or profile.
    /// </summary>
    double Station { get; }

    /// <summary>
    /// Gets the elevation at this station.
    /// </summary>
    double Elevation { get; }

    /// <summary>
    /// Converts this station point to a Rhino Point3d in profile space.
    /// </summary>
    /// <returns>A Point3d with Station as X, Elevation as Y, and Z as 0.</returns>
    Point3d ToRhinoPoint3d();
}
