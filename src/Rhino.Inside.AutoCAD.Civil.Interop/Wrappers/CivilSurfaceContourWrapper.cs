using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps contour data extracted from a Civil 3D TIN Surface.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted contour information.
/// Unlike other Civil 3D wrappers, this does not wrap a database object since
/// contours are extracted as temporary geometry from a TinSurface.
/// </remarks>
public class CivilSurfaceContourWrapper : ICivilSurfaceContour
{
    /// <inheritdoc />
    public int ContourType { get; }

    /// <inheritdoc />
    public Curve Curve { get; }

    /// <inheritdoc />
    public double Elevation { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilSurfaceContourWrapper"/>.
    /// </summary>
    /// <param name="contourType">
    /// The contour type (0=All, 1=Major, 2=Minor).
    /// </param>
    /// <param name="curve">
    /// The contour geometry as a Rhino curve.
    /// </param>
    /// <param name="elevation">
    /// The elevation of the contour line.
    /// </param>
    public CivilSurfaceContourWrapper(int contourType, Curve curve, double elevation)
    {
        ContourType = contourType;
        Curve = curve;
        Elevation = elevation;
    }

    /// <summary>
    /// Creates a duplicate of this contour wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilSurfaceContourWrapper Duplicate()
    {
        var curveCopy = Curve.DuplicateCurve();
        return new CivilSurfaceContourWrapper(ContourType, curveCopy, Elevation);
    }

    /// <summary>
    /// Gets a human-readable description of the contour type.
    /// </summary>
    public string ContourTypeName => ContourType switch
    {
        0 => "All",
        1 => "Major",
        2 => "Minor",
        _ => "Unknown"
    };

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Surface Contour [{ContourTypeName}] Elev: {Elevation:F3}";
    }
}
