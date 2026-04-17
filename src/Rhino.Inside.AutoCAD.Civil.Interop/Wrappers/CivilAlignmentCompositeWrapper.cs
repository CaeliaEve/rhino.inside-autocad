using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps composite sub-entity data extracted from a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted alignment composite information.
/// Composite entities represent complex geometry that may contain multiple sub-components
/// (such as spiral-curve-spiral groups).
/// </remarks>
public class CivilAlignmentCompositeWrapper : ICivilAlignmentComposite
{
    /// <inheritdoc />
    public PolyCurve Curve { get; }

    /// <inheritdoc />
    public double StartStation { get; }

    /// <inheritdoc />
    public double EndStation { get; }

    /// <inheritdoc />
    public double Length { get; }

    /// <inheritdoc />
    public int ComponentCount { get; }

    /// <inheritdoc />
    public int Index { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilAlignmentCompositeWrapper"/>.
    /// </summary>
    /// <param name="curve">The composite geometry as a Rhino polycurve.</param>
    /// <param name="startStation">The starting station along the alignment.</param>
    /// <param name="endStation">The ending station along the alignment.</param>
    /// <param name="length">The length of the composite segment.</param>
    /// <param name="componentCount">The number of component segments in the composite.</param>
    /// <param name="index">The index of this sub-entity within the alignment.</param>
    public CivilAlignmentCompositeWrapper(
        PolyCurve curve,
        double startStation,
        double endStation,
        double length,
        int componentCount,
        int index)
    {
        Curve = curve;
        StartStation = startStation;
        EndStation = endStation;
        Length = length;
        ComponentCount = componentCount;
        Index = index;
    }

    /// <summary>
    /// Creates a duplicate of this alignment composite wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilAlignmentCompositeWrapper Duplicate()
    {
        var curveCopy = Curve.DuplicatePolyCurve();
        return new CivilAlignmentCompositeWrapper(
            curveCopy, StartStation, EndStation, Length, ComponentCount, Index);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Alignment Composite [Index: {Index}] Sta {StartStation:F2} - {EndStation:F2}, {ComponentCount} components";
    }
}
