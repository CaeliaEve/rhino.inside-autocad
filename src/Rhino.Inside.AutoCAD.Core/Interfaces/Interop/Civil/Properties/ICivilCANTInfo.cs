namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents CANT (superelevation) information for a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// Bundles CANT critical stations and curve information for rail alignments.
/// </remarks>
public interface ICivilCantInfo
{
    /// <summary>
    /// Gets a value indicating whether this alignment has CANT data.
    /// </summary>
    bool HasCantInfo { get; }

    /// <summary>
    /// Gets the collection of CANT critical stations along the alignment.
    /// </summary>
    IReadOnlyList<ICivilCantCriticalStation> CriticalStations { get; }

    /// <summary>
    /// Gets the collection of CANT curves along the alignment.
    /// </summary>
    IReadOnlyList<ICivilCantCurve> Curves { get; }

    /// <summary>
    /// Creates a shallow copy of this CANT information.
    /// </summary>
    /// <returns>A new instance with the same values.</returns>
    ICivilCantInfo ShallowClone();
}

