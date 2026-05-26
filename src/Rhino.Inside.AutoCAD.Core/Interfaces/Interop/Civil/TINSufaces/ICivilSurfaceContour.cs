using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a contour line extracted from a Civil 3D TIN Surface.
/// </summary>
/// <remarks>
/// Surface contours in Civil 3D represent lines of equal elevation on a TIN surface.
/// This interface provides access to the contour's geometry, type, and elevation information.
/// </remarks>
public interface ICivilSurfaceContour
{
    /// <summary>
    /// Gets the contour type (Major or Minor).
    /// </summary>
    CivilContourType CivilContourType { get; }

    /// <summary>
    /// Gets the contour geometry as a Rhino curve.
    /// </summary>
    Curve Curve { get; }

    /// <summary>
    /// Gets the elevation of the contour line.
    /// </summary>
    double Elevation { get; }
}
