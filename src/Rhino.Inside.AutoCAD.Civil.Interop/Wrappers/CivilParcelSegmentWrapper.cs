using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps a boundary segment extracted from a Civil 3D Parcel.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted parcel segment information.
/// Unlike other Civil 3D wrappers, this does not wrap a database object since
/// parcel segments are extracted as temporary geometry from a Parcel.
/// </remarks>
public class CivilParcelSegmentWrapper : ICivilParcelSegment
{
    /// <inheritdoc />
    public string SegmentType { get; }

    /// <inheritdoc />
    public double Length { get; }

    /// <inheritdoc />
    public double Direction { get; }

    /// <inheritdoc />
    public double Radius { get; }

    /// <inheritdoc />
    public int Index { get; }

    /// <inheritdoc />
    public Curve Curve { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilParcelSegmentWrapper"/>.
    /// </summary>
    /// <param name="segmentType">The type of segment (Line, Arc, Spiral).</param>
    /// <param name="length">The length of the segment.</param>
    /// <param name="direction">The direction (bearing) of the segment in radians.</param>
    /// <param name="radius">The radius (0 for lines).</param>
    /// <param name="index">The index of this segment in the parcel boundary.</param>
    /// <param name="curve">The geometry as a Rhino curve.</param>
    public CivilParcelSegmentWrapper(
        string segmentType,
        double length,
        double direction,
        double radius,
        int index,
        Curve curve)
    {
        SegmentType = segmentType;
        Length = length;
        Direction = direction;
        Radius = radius;
        Index = index;
        Curve = curve;
    }

    /// <summary>
    /// Creates a duplicate of this parcel segment wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilParcelSegmentWrapper Duplicate()
    {
        var curveCopy = Curve.DuplicateCurve();
        return new CivilParcelSegmentWrapper(
            SegmentType,
            Length,
            Direction,
            Radius,
            Index,
            curveCopy);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Parcel Segment [{SegmentType}] (Index: {Index}, Length: {Length:F2})";
    }
}
