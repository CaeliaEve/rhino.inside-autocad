using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core;
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
public class CivilSurfaceContour : ICivilSurfaceContour
{
    /// <inheritdoc />
    public CivilContourType CivilContourType { get; }

    /// <inheritdoc />
    public Curve Curve { get; }

    /// <inheritdoc />
    public double Elevation { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilSurfaceContour"/>.
    /// </summary>
    /// <param name="civilContourType">
    /// The contour type (Major or Minor).
    /// </param>
    /// <param name="curve">
    /// The contour geometry as a Rhino curve.
    /// </param>
    public CivilSurfaceContour(CivilContourType civilContourType, Curve curve)
    {
        this.CivilContourType = civilContourType;
        this.Curve = curve;
        this.Elevation = curve.PointAtStart.Z;
    }

    /// <summary>
    /// Creates a duplicate of this contour wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilSurfaceContour Duplicate()
    {
        var curveCopy = this.Curve.DuplicateCurve();
        return new CivilSurfaceContour(this.CivilContourType, curveCopy);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Surface Contour [{this.CivilContourType}] Elev: {this.Elevation:F3}";
    }
}
