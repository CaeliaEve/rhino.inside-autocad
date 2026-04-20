namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents properties extracted from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// This interface provides access to profile metadata and statistics
/// without requiring direct access to the Civil 3D database object.
/// </remarks>
public interface ICivilProfileProperties
{
    /// <summary>
    /// Gets the name of the profile.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the profile.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the starting station of the profile.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the ending station of the profile.
    /// </summary>
    double EndStation { get; }

    /// <summary>
    /// Gets the minimum elevation of the profile.
    /// </summary>
    double MinElevation { get; }

    /// <summary>
    /// Gets the maximum elevation of the profile.
    /// </summary>
    double MaxElevation { get; }

    /// <summary>
    /// Gets the profile type.
    /// </summary>
    CivilProfileType ProfileType { get; }

    /// <summary>
    /// Gets the number of entities (segments) in the profile.
    /// </summary>
    int EntityCount { get; }

    /// <summary>
    /// Gets the name of the parent alignment containing this profile.
    /// </summary>
    string ParentAlignmentName { get; }
}
