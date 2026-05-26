using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Represents a CANT curve.
/// </summary>
public class CivilCantCurve : AutocadWrapperBase<CANTCurve>, ICivilCantCurve
{
    private readonly CANTCurve _cantCurve;

    /// <inheritdoc />
    public double StartStation { get; }

    /// <inheritdoc />
    public double EndStation { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilCantCurve"/>.
    /// </summary>
    public CivilCantCurve(CANTCurve cantCurve) : base(cantCurve)
    {
        _cantCurve = cantCurve;
        this.StartStation = cantCurve.StartStation;
        this.EndStation = cantCurve.EndStation;
    }

    /// <summary>
    /// Returns a string representation of this CANT curve.
    /// </summary>
    public override string ToString()
    {
        return $"Curve Sta {this.StartStation:F2}-{this.EndStation:F2}";
    }
}
