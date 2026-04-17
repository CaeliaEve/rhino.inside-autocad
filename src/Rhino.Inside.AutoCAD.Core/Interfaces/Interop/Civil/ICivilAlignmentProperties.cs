namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents properties extracted from a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// This interface provides access to alignment metadata and statistics
/// without requiring direct access to the Civil 3D database object.
/// </remarks>
public interface ICivilAlignmentProperties
{
    /// <summary>
    /// Gets the name of the alignment.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the alignment.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the starting station of the alignment.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the ending station of the alignment.
    /// </summary>
    double EndStation { get; }

    /// <summary>
    /// Gets the total length of the alignment.
    /// </summary>
    double Length { get; }

    /// <summary>
    /// Gets the alignment type as an integer.
    /// </summary>
    /// <value>
    /// Common values: 0 = Centerline, 1 = Offset, 2 = CurbReturn, etc.
    /// </value>
    int AlignmentType { get; }

    /// <summary>
    /// Gets the number of entities (segments) in the alignment.
    /// </summary>
    int EntityCount { get; }

    /// <summary>
    /// Gets the name of the site containing this alignment.
    /// </summary>
    string SiteName { get; }
}
