using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Represents a design speed at a specific station.
/// </summary>
public class CivilDesignSpeedStation : AutocadWrapperBase<DesignSpeed>, ICivilDesignSpeedStation
{
    /// <inheritdoc />
    public double Station { get; }

    /// <inheritdoc />
    public double Speed { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilDesignSpeedStation"/>.
    /// </summary>
    public CivilDesignSpeedStation(DesignSpeed speed) : base(speed)
    {
        this.Station = speed.Station;
        this.Speed = speed.SpeedNumber;
    }

    /// <summary>
    /// Returns a string representation of this speed station.
    /// </summary>
    public override string ToString()
    {
        return $"Station {this.Station:F2}: {this.Speed:F2}";
    }
}
