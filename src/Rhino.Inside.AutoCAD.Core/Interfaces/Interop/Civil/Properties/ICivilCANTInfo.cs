namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents CANT (superelevation) information for a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// Bundles CANT critical stations and curve information for rail alignments.
/// </remarks>
public interface ICivilCANTInfo
{
    /// <summary>
    /// Gets a value indicating whether this alignment has CANT data.
    /// </summary>
    bool HasCANT { get; }

    /// <summary>
    /// Gets the collection of CANT critical stations along the alignment.
    /// </summary>
    IReadOnlyList<ICivilCANTCriticalStation> CriticalStations { get; }

    /// <summary>
    /// Gets the collection of CANT curves along the alignment.
    /// </summary>
    IReadOnlyList<ICivilCANTCurve> Curves { get; }

    /// <summary>
    /// Creates a shallow copy of this CANT information.
    /// </summary>
    /// <returns>A new instance with the same values.</returns>
    ICivilCANTInfo ShallowClone();
}

/// <summary>
/// Represents a CANT critical station along an alignment.
/// </summary>
public interface ICivilCANTCriticalStation
{
    /// <summary>
    /// Gets the station value of this critical station.
    /// </summary>
    double Station { get; }

    /// <summary>
    /// Gets the type of this critical station (e.g., "Begin", "End", "Full").
    /// </summary>
    string StationType { get; }

    /// <summary>
    /// Gets the CANT value at this station.
    /// </summary>
    double Cant { get; }

    /// <summary>
    /// Gets the pivot value at this station.
    /// </summary>
    double Pivot { get; }
}

/// <summary>
/// Represents a CANT curve along an alignment.
/// </summary>
public interface ICivilCANTCurve
{
    /// <summary>
    /// Gets the starting station of this CANT curve.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the ending station of this CANT curve.
    /// </summary>
    double EndStation { get; }

    /// <summary>
    /// Gets the radius of the curve.
    /// </summary>
    double Radius { get; }

    /// <summary>
    /// Gets the design CANT value for this curve.
    /// </summary>
    double DesignCant { get; }

    /// <summary>
    /// Gets the applied CANT value for this curve.
    /// </summary>
    double AppliedCant { get; }
}
