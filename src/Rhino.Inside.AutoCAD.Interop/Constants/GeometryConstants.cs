using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Provides standard tolerance and precision constants for geometric operations.
/// </summary>
/// <remarks>
/// These constants define the precision thresholds used throughout the interop layer
/// when converting geometry between Rhino and AutoCAD, performing comparisons,
/// and fitting curves to control points.
/// </remarks>
public class GeometryConstants
{
    /// <summary>
    /// The fit tolerance used when approximating NURBS and spline curves.
    /// </summary>
    /// <remarks>
    /// This tolerance (0.001) controls how closely a fitted curve must match the original
    /// geometry. Smaller values produce more accurate but potentially more complex curves.
    /// </remarks>
    /// <seealso cref="NurbsCurve"/>
    public const double FitTolerance = 0.001;

    /// <summary>
    /// The normalized parameter value representing the midpoint of a <see cref="Curve"/>.
    /// </summary>
    /// <remarks>
    /// A value of 0.5 corresponds to the exact center along the curve's domain,
    /// commonly used when evaluating the midpoint <see cref="Point3d"/> of a curve.
    /// </remarks>
    public const double NormalizedMidLength = 0.5;

    /// <summary>
    /// Tolerance threshold for determining if a length is effectively zero.
    /// </summary>
    /// <remarks>
    /// Values below this threshold (0.0001) are treated as zero-length,
    /// which helps avoid division-by-zero errors and degenerate geometry conditions.
    /// </remarks>
    /// <seealso cref="VertexTolerance"/>
    public const double ZeroTolerance = 0.0001;

    /// <summary>
    /// A minimal tolerance for ratio and proportion comparisons.
    /// </summary>
    /// <remarks>
    /// This extremely small value (1e-10) accounts for floating-point precision errors
    /// when comparing ratios or normalized values that should theoretically be equal.
    /// </remarks>
    public const double RatioTolerance = 1e-10;

    /// <summary>
    /// Tolerance for comparing vertex positions, such as <see cref="Mesh"/> vertices or control points.
    /// </summary>
    /// <remarks>
    /// Points within this distance (0.0001) are considered coincident.
    /// Used for vertex welding, duplicate detection, and geometric comparisons.
    /// </remarks>
    public const double VertexTolerance = 0.0001;
}