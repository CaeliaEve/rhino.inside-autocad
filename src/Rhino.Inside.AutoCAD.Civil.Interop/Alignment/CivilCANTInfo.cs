using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps CANT (superelevation) information extracted from a Civil 3D Alignment.
/// </summary>
public record CivilCANTInfo : ICivilCANTInfo
{
    /// <inheritdoc />
    public bool HasCANT { get; }

    /// <inheritdoc />
    public IReadOnlyList<ICivilCANTCriticalStation> CriticalStations { get; }

    /// <inheritdoc />
    public IReadOnlyList<ICivilCANTCurve> Curves { get; }

    /// <summary>
    /// Gets an empty CANT info instance with no data.
    /// </summary>
    public static CivilCANTInfo Empty { get; } = new();

    /// <summary>
    /// Initializes a new empty instance of <see cref="CivilCANTInfo"/>.
    /// </summary>
    public CivilCANTInfo()
    {
        HasCANT = false;
        CriticalStations = Array.Empty<ICivilCANTCriticalStation>();
        Curves = Array.Empty<ICivilCANTCurve>();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilCANTInfo"/> from an Alignment.
    /// </summary>
    /// <param name="alignment">The Civil 3D alignment to extract CANT from.</param>
    public CivilCANTInfo(Alignment alignment)
    {
        try
        {
            var cantInfo = alignment.GetCANT();

            if (cantInfo == null)
            {
                HasCANT = false;
                CriticalStations = Array.Empty<ICivilCANTCriticalStation>();
                Curves = Array.Empty<ICivilCANTCurve>();
                return;
            }

            HasCANT = true;

            var criticalStations = new List<ICivilCANTCriticalStation>();
            var cantCriticalStations = cantInfo.GetCANTCriticalStations();

            if (cantCriticalStations != null)
            {
                foreach (var station in cantCriticalStations)
                {
                    criticalStations.Add(new CivilCANTCriticalStation(
                        station.Station,
                        station.StationType.ToString(),
                        station.Cant,
                        station.Pivot));
                }
            }

            CriticalStations = criticalStations;

            var curves = new List<ICivilCANTCurve>();
            var cantCurves = cantInfo.GetCANTCurves();

            if (cantCurves != null)
            {
                foreach (var curve in cantCurves)
                {
                    curves.Add(new CivilCANTCurve(
                        curve.StartStation,
                        curve.EndStation,
                        curve.Radius,
                        curve.DesignCant,
                        curve.AppliedCant));
                }
            }

            Curves = curves;
        }
        catch
        {
            HasCANT = false;
            CriticalStations = Array.Empty<ICivilCANTCriticalStation>();
            Curves = Array.Empty<ICivilCANTCurve>();
        }
    }

    /// <inheritdoc />
    public ICivilCANTInfo ShallowClone()
    {
        return this with { };
    }

    /// <summary>
    /// Returns a string representation of this CANT information.
    /// </summary>
    public override string ToString()
    {
        if (!HasCANT)
            return "No CANT Data";

        return $"CANT Info: {CriticalStations.Count} critical stations, {Curves.Count} curves";
    }
}

/// <summary>
/// Represents a CANT critical station.
/// </summary>
public record CivilCANTCriticalStation : ICivilCANTCriticalStation
{
    /// <inheritdoc />
    public double Station { get; }

    /// <inheritdoc />
    public string StationType { get; }

    /// <inheritdoc />
    public double Cant { get; }

    /// <inheritdoc />
    public double Pivot { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilCANTCriticalStation"/>.
    /// </summary>
    public CivilCANTCriticalStation(double station, string stationType, double cant, double pivot)
    {
        Station = station;
        StationType = stationType ?? string.Empty;
        Cant = cant;
        Pivot = pivot;
    }

    /// <summary>
    /// Returns a string representation of this critical station.
    /// </summary>
    public override string ToString()
    {
        return $"{StationType} at Station {Station:F2}: Cant={Cant:F4}, Pivot={Pivot:F4}";
    }
}

/// <summary>
/// Represents a CANT curve.
/// </summary>
public record CivilCANTCurve : ICivilCANTCurve
{
    /// <inheritdoc />
    public double StartStation { get; }

    /// <inheritdoc />
    public double EndStation { get; }

    /// <inheritdoc />
    public double Radius { get; }

    /// <inheritdoc />
    public double DesignCant { get; }

    /// <inheritdoc />
    public double AppliedCant { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilCANTCurve"/>.
    /// </summary>
    public CivilCANTCurve(double startStation, double endStation, double radius, double designCant, double appliedCant)
    {
        StartStation = startStation;
        EndStation = endStation;
        Radius = radius;
        DesignCant = designCant;
        AppliedCant = appliedCant;
    }

    /// <summary>
    /// Returns a string representation of this CANT curve.
    /// </summary>
    public override string ToString()
    {
        return $"Curve Sta {StartStation:F2}-{EndStation:F2}: R={Radius:F2}, Design={DesignCant:F4}, Applied={AppliedCant:F4}";
    }
}
