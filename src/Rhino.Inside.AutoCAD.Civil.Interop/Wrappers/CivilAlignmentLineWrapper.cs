using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps line sub-entity data extracted from a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted alignment line information.
/// Unlike other Civil 3D wrappers, this does not wrap a database object since
/// alignment sub-entities are extracted as temporary geometry from an Alignment.
/// </remarks>
public class CivilAlignmentLineWrapper : ICivilAlignmentLine
{
    /// <inheritdoc />
    public Line Line { get; }

    /// <inheritdoc />
    public double StartStation { get; }

    /// <inheritdoc />
    public double EndStation { get; }

    /// <inheritdoc />
    public double Length { get; }

    /// <inheritdoc />
    public int Index { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilAlignmentLineWrapper"/>.
    /// </summary>
    /// <param name="line">The line geometry.</param>
    /// <param name="startStation">The starting station along the alignment.</param>
    /// <param name="endStation">The ending station along the alignment.</param>
    /// <param name="length">The length of the line segment.</param>
    /// <param name="index">The index of this sub-entity within the alignment.</param>
    public CivilAlignmentLineWrapper(Line line, double startStation, double endStation, double length, int index)
    {
        Line = line;
        StartStation = startStation;
        EndStation = endStation;
        Length = length;
        Index = index;
    }

    /// <summary>
    /// Creates a duplicate of this alignment line wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilAlignmentLineWrapper Duplicate()
    {
        return new CivilAlignmentLineWrapper(Line, StartStation, EndStation, Length, Index);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Alignment Line [Index: {Index}] Sta {StartStation:F2} - {EndStation:F2}, Length: {Length:F2}";
    }
}
