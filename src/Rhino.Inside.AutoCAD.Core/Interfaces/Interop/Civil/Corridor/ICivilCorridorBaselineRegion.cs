namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a baseline region extracted from a Civil 3D Corridor.
/// </summary>
/// <remarks>
/// A baseline region defines a portion of a baseline where a specific
/// assembly is applied to generate the corridor cross-sections.
/// </remarks>
public interface ICivilCorridorBaselineRegion
{
    /// <summary>
    /// Gets the name of the baseline region.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the ObjectId of the assembly applied to this region.
    /// </summary>
    IObjectId AssemblyId { get; }

    /// <summary>
    /// Gets the starting station of the region.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the ending station of the region.
    /// </summary>
    double EndStation { get; }

    /// <summary>
    /// Gets the length of the region.
    /// </summary>
    double Length { get; }
}
