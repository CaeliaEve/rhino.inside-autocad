using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a contour line extracted from a Civil 3D TIN Surface.
/// </summary>
/// <remarks>
/// Surface contours in Civil 3D represent lines of equal elevation on a TIN surface.
/// This interface provides access to the contour's geometry, type, and elevation information.
/// <para>
/// Contour types are represented as integers:
/// <list type="bullet">
/// <item><description>0 = All contours</description></item>
/// <item><description>1 = Major contours</description></item>
/// <item><description>2 = Minor contours</description></item>
/// </list>
/// </para>
/// </remarks>
public interface ICivilSurfaceContour
{
    /// <summary>
    /// Gets the contour type as an integer.
    /// </summary>
    /// <value>
    /// 0 = All, 1 = Major, 2 = Minor
    /// </value>
    int ContourType { get; }

    /// <summary>
    /// Gets the contour geometry as a Rhino curve.
    /// </summary>
    Curve Curve { get; }

    /// <summary>
    /// Gets the elevation of the contour line.
    /// </summary>
    double Elevation { get; }
}
