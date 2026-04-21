namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents properties extracted from a Civil 3D Corridor.
/// </summary>
/// <remarks>
/// This interface provides access to corridor metadata and statistics
/// without requiring direct access to the Civil 3D database object.
/// </remarks>
public interface ICivilCorridorProperties
{
    /// <summary>
    /// Gets the name of the corridor.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the corridor.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the starting station of the corridor.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the ending station of the corridor.
    /// </summary>
    double EndStation { get; }

    /// <summary>
    /// Gets the total length of the corridor.
    /// </summary>
    double Length { get; }
}
