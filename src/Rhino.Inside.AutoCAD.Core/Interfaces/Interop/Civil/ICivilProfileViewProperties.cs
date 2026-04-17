namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents properties extracted from a Civil 3D ProfileView.
/// </summary>
/// <remarks>
/// This interface provides access to ProfileView metadata and display settings
/// without requiring direct access to the Civil 3D database object.
/// </remarks>
public interface ICivilProfileViewProperties
{
    /// <summary>
    /// Gets the name of the ProfileView.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the ProfileView.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the starting station of the ProfileView display range.
    /// </summary>
    double StationStart { get; }

    /// <summary>
    /// Gets the ending station of the ProfileView display range.
    /// </summary>
    double StationEnd { get; }

    /// <summary>
    /// Gets the minimum elevation of the ProfileView display range.
    /// </summary>
    double ElevationMin { get; }

    /// <summary>
    /// Gets the maximum elevation of the ProfileView display range.
    /// </summary>
    double ElevationMax { get; }

    /// <summary>
    /// Gets the name of the parent alignment for this ProfileView.
    /// </summary>
    string AlignmentName { get; }

    /// <summary>
    /// Gets the number of profiles displayed in this ProfileView.
    /// </summary>
    int ProfileCount { get; }

    /// <summary>
    /// Gets the number of bands (top and bottom) in this ProfileView.
    /// </summary>
    int BandCount { get; }

    /// <summary>
    /// Gets the horizontal scale of the ProfileView.
    /// </summary>
    double HorizontalScale { get; }

    /// <summary>
    /// Gets the vertical scale of the ProfileView.
    /// </summary>
    double VerticalScale { get; }

    /// <summary>
    /// Gets the vertical exaggeration factor of the ProfileView.
    /// </summary>
    double VerticalExaggeration { get; }
}
