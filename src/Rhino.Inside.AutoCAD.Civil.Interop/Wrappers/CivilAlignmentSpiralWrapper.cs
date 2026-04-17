using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps spiral sub-entity data extracted from a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted alignment spiral information.
/// Unlike other Civil 3D wrappers, this does not wrap a database object since
/// alignment sub-entities are extracted as temporary geometry from an Alignment.
/// </remarks>
public class CivilAlignmentSpiralWrapper : ICivilAlignmentSpiral
{
    /// <inheritdoc />
    public Curve Curve { get; }

    /// <inheritdoc />
    public double StartStation { get; }

    /// <inheritdoc />
    public double EndStation { get; }

    /// <inheritdoc />
    public double Length { get; }

    /// <inheritdoc />
    public double RadiusIn { get; }

    /// <inheritdoc />
    public double RadiusOut { get; }

    /// <inheritdoc />
    public string SpiralType { get; }

    /// <inheritdoc />
    public bool IsClockwise { get; }

    /// <inheritdoc />
    public int Index { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilAlignmentSpiralWrapper"/>.
    /// </summary>
    /// <param name="curve">The spiral geometry as a Rhino curve.</param>
    /// <param name="startStation">The starting station along the alignment.</param>
    /// <param name="endStation">The ending station along the alignment.</param>
    /// <param name="length">The length of the spiral segment.</param>
    /// <param name="radiusIn">The radius at the start of the spiral.</param>
    /// <param name="radiusOut">The radius at the end of the spiral.</param>
    /// <param name="spiralType">The spiral definition type name.</param>
    /// <param name="isClockwise">Whether the spiral curves clockwise.</param>
    /// <param name="index">The index of this sub-entity within the alignment.</param>
    public CivilAlignmentSpiralWrapper(
        Curve curve,
        double startStation,
        double endStation,
        double length,
        double radiusIn,
        double radiusOut,
        string spiralType,
        bool isClockwise,
        int index)
    {
        Curve = curve;
        StartStation = startStation;
        EndStation = endStation;
        Length = length;
        RadiusIn = radiusIn;
        RadiusOut = radiusOut;
        SpiralType = spiralType;
        IsClockwise = isClockwise;
        Index = index;
    }

    /// <summary>
    /// Creates a duplicate of this alignment spiral wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilAlignmentSpiralWrapper Duplicate()
    {
        var curveCopy = Curve.DuplicateCurve();
        return new CivilAlignmentSpiralWrapper(
            curveCopy, StartStation, EndStation, Length,
            RadiusIn, RadiusOut, SpiralType, IsClockwise, Index);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var direction = IsClockwise ? "CW" : "CCW";
        return $"Alignment Spiral [Index: {Index}] Sta {StartStation:F2} - {EndStation:F2}, {SpiralType} ({direction})";
    }
}
