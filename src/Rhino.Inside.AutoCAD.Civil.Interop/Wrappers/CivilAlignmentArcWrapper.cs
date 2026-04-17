using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps arc sub-entity data extracted from a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted alignment arc information.
/// Unlike other Civil 3D wrappers, this does not wrap a database object since
/// alignment sub-entities are extracted as temporary geometry from an Alignment.
/// </remarks>
public class CivilAlignmentArcWrapper : ICivilAlignmentArc
{
    /// <inheritdoc />
    public Arc Arc { get; }

    /// <inheritdoc />
    public double StartStation { get; }

    /// <inheritdoc />
    public double EndStation { get; }

    /// <inheritdoc />
    public double Length { get; }

    /// <inheritdoc />
    public double Radius { get; }

    /// <inheritdoc />
    public Point3d CenterPoint { get; }

    /// <inheritdoc />
    public bool IsClockwise { get; }

    /// <inheritdoc />
    public int Index { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilAlignmentArcWrapper"/>.
    /// </summary>
    /// <param name="arc">The arc geometry.</param>
    /// <param name="startStation">The starting station along the alignment.</param>
    /// <param name="endStation">The ending station along the alignment.</param>
    /// <param name="length">The length of the arc segment.</param>
    /// <param name="radius">The radius of the arc.</param>
    /// <param name="centerPoint">The center point of the arc.</param>
    /// <param name="isClockwise">Whether the arc curves clockwise.</param>
    /// <param name="index">The index of this sub-entity within the alignment.</param>
    public CivilAlignmentArcWrapper(
        Arc arc,
        double startStation,
        double endStation,
        double length,
        double radius,
        Point3d centerPoint,
        bool isClockwise,
        int index)
    {
        Arc = arc;
        StartStation = startStation;
        EndStation = endStation;
        Length = length;
        Radius = radius;
        CenterPoint = centerPoint;
        IsClockwise = isClockwise;
        Index = index;
    }

    /// <summary>
    /// Creates a duplicate of this alignment arc wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilAlignmentArcWrapper Duplicate()
    {
        return new CivilAlignmentArcWrapper(Arc, StartStation, EndStation, Length, Radius, CenterPoint, IsClockwise, Index);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var direction = IsClockwise ? "CW" : "CCW";
        return $"Alignment Arc [Index: {Index}] Sta {StartStation:F2} - {EndStation:F2}, R={Radius:F2} ({direction})";
    }
}
