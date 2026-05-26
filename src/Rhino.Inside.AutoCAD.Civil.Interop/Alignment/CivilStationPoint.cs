using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Represents a station value with its corresponding elevation.
/// </summary>
public record CivilStationPoint : ICivilStationPoint
{
    /// <inheritdoc />
    public double Station { get; }

    /// <inheritdoc />
    public double Elevation { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilStationPoint"/>.
    /// </summary>
    /// <param name="station">The station value.</param>
    /// <param name="elevation">The elevation at this station.</param>
    public CivilStationPoint(double station, double elevation)
    {
        this.Station = station;
        this.Elevation = elevation;
    }

    /// <inheritdoc />
    public Point3d ToRhinoPoint3d()
    {
        return new Point3d(this.Station, this.Elevation, 0);
    }

    /// <summary>
    /// Returns a string representation of this station point.
    /// </summary>
    public override string ToString()
    {
        return $"Station {this.Station:F2}: Elevation {this.Elevation:F2}";
    }
}
