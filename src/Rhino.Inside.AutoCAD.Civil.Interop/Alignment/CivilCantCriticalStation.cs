using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Represents a CANT critical station.
/// </summary>
public class CivilCantCriticalStation : AutocadWrapperBase<CANTCriticalStation>, ICivilCantCriticalStation
{
    /// <inheritdoc />
    public double Station { get; }

    /// <inheritdoc />
    public string StationType { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilCantCriticalStation"/>.
    /// </summary>
    public CivilCantCriticalStation(CANTCriticalStation cantStation) : base(cantStation)
    {
        this.Station = cantStation.Station;
        this.StationType = cantStation.StationType.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Returns a string representation of this critical station.
    /// </summary>
    public override string ToString()
    {
        return $"{this.StationType} at Station {this.Station:F2}";
    }
}