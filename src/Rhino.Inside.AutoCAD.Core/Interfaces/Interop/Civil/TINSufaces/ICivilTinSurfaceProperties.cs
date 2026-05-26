namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents general statistics and properties of a Civil 3D TIN Surface.
/// </summary>
/// <remarks>
/// TIN properties provide surface analysis data including elevation range
/// and basic statistics.
/// </remarks>
public interface ICivilTinSurfaceProperties
{
    /// <summary>
    /// Gets the name of the surface.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the minimum surface point (minimum X, Y, and elevation values).
    /// </summary>
    /// <remarks>
    /// This point represents the corner of the surface bounding box with the smallest coordinate values.
    /// </remarks>
    ICivilSurfacePoint MinimumPoint { get; }

    /// <summary>
    /// Gets the maximum surface point (maximum X, Y, and elevation values).
    /// </summary>
    /// <remarks>
    /// This point represents the corner of the surface bounding box with the largest coordinate values.
    /// </remarks>
    ICivilSurfacePoint MaximumPoint { get; }

    /// <summary>
    /// Gets the style applied to this TIN surface as a NamedId.
    /// </summary>
    /// <remarks>
    /// Provides both the style name and ObjectId reference.
    /// </remarks>
    INamedId Style { get; }
}
