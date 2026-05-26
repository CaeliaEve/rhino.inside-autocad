using Rhino.Inside.AutoCAD.Core.Interfaces;
using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps a boundary segment extracted from a Civil 3D Parcel.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted parcel segment information.
/// Unlike other Civil 3D wrappers, this does not wrap a database object since
/// parcel segments are extracted as temporary geometry from a Parcel.
/// </remarks>
public class CivilParcelSegment : ICivilParcelSegment
{
    /// <inheritdoc />
    public int Index { get; }

    /// <inheritdoc />
    public RhinoCurve Curve { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilParcelSegment"/>.
    /// </summary>
    /// <param name="index">The index of this segment in the parcel boundary.</param>
    /// <param name="rhinoCurve">The geometry as a Rhino curve.</param>
    public CivilParcelSegment(RhinoCurve rhinoCurve, int index)
    {
        this.Index = index;
        this.Curve = rhinoCurve;
    }

    /// <summary>
    /// Creates a duplicate of this parcel segment wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilParcelSegment ShallowClone()
    {
        return new CivilParcelSegment(this.Curve.DuplicateCurve(), this.Index);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Parcel Segment [{this.Curve.GetType().Name}] (Index: {this.Index})";
    }
}
