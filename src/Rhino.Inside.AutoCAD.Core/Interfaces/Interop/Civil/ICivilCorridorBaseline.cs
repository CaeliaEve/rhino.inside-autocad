namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a baseline extracted from a Civil 3D Corridor.
/// </summary>
/// <remarks>
/// A baseline consists of an alignment and profile pair that defines
/// the horizontal and vertical path of the corridor.
/// </remarks>
public interface ICivilCorridorBaseline
{
    /// <summary>
    /// Gets the name of the baseline.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the ObjectId of the alignment associated with this baseline.
    /// </summary>
    IObjectId AlignmentId { get; }

    /// <summary>
    /// Gets the ObjectId of the profile associated with this baseline.
    /// </summary>
    IObjectId ProfileId { get; }

    /// <summary>
    /// Gets the starting station of the baseline.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the ending station of the baseline.
    /// </summary>
    double EndStation { get; }

    /// <summary>
    /// Gets the number of regions in this baseline.
    /// </summary>
    int RegionCount { get; }
}
