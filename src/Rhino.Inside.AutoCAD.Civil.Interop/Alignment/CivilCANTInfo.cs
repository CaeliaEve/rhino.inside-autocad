using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps CANT (superelevation) information extracted from a Civil 3D Alignment.
/// </summary>
public record CivilCantInfo : ICivilCantInfo
{
    /// <inheritdoc />
    public bool HasCantInfo { get; }

    /// <inheritdoc />
    public IReadOnlyList<ICivilCantCriticalStation> CriticalStations { get; }

    /// <inheritdoc />
    public IReadOnlyList<ICivilCantCurve> Curves { get; }

    /// <summary>
    /// Gets an empty CANT info instance with no data.
    /// </summary>
    public static CivilCantInfo Empty { get; } = new();

    /// <summary>
    /// Initializes a new empty instance of <see cref="CivilCantInfo"/>.
    /// </summary>
    public CivilCantInfo()
    {
        this.HasCantInfo = false;
        this.CriticalStations = Array.Empty<ICivilCantCriticalStation>();
        this.Curves = Array.Empty<ICivilCantCurve>();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilCantInfo"/> from an Alignment.
    /// </summary>
    /// <param name="alignment">The Civil 3D alignment to extract CANT from.</param>
    public CivilCantInfo(Alignment alignment)
    {
        var cantCurveCollection = alignment.CANTCurves;
        var cantStations = alignment.CANTCriticalStaitons;

        var cantCriticalStations = new List<ICivilCantCriticalStation>();

        if (cantStations != null)
        {
            foreach (var station in cantStations)
            {
                cantCriticalStations.Add(new CivilCantCriticalStation(station));
            }
        }

        this.CriticalStations = cantCriticalStations;

        var curves = new List<ICivilCantCurve>();

        if (cantCurveCollection != null)
        {
            foreach (var curve in cantCurveCollection)
            {
                curves.Add(new CivilCantCurve(curve));
            }
        }

        this.Curves = curves;

        this.HasCantInfo = cantCriticalStations.Any() && curves.Any();

    }

    /// <inheritdoc />
    public ICivilCantInfo ShallowClone()
    {
        return this with { };
    }

    /// <summary>
    /// Returns a string representation of this CANT information.
    /// </summary>
    public override string ToString()
    {
        if (!this.HasCantInfo)
            return "No CANT Data";

        return $"CANT Info: {this.CriticalStations.Count} critical stations, {this.Curves.Count} curves";
    }
}
