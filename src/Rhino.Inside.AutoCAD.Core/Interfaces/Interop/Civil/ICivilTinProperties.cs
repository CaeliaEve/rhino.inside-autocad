namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents general statistics and properties of a Civil 3D TIN Surface.
/// </summary>
/// <remarks>
/// TIN properties provide surface analysis data including elevation range
/// and basic statistics.
/// </remarks>
public interface ICivilTinProperties
{
    /// <summary>
    /// Gets the name of the surface.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the minimum elevation of the surface.
    /// </summary>
    double MinimumElevation { get; }

    /// <summary>
    /// Gets the maximum elevation of the surface.
    /// </summary>
    double MaximumElevation { get; }

    /// <summary>
    /// Gets the minimum X coordinate of the surface extent.
    /// </summary>
    double MinimumX { get; }

    /// <summary>
    /// Gets the maximum X coordinate of the surface extent.
    /// </summary>
    double MaximumX { get; }

    /// <summary>
    /// Gets the minimum Y coordinate of the surface extent.
    /// </summary>
    double MinimumY { get; }

    /// <summary>
    /// Gets the maximum Y coordinate of the surface extent.
    /// </summary>
    double MaximumY { get; }
}
