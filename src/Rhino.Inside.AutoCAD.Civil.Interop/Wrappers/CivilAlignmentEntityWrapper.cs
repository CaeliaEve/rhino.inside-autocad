using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps an individual entity (segment) extracted from a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted alignment entity information.
/// Unlike other Civil 3D wrappers, this does not wrap a database object since
/// alignment entities are extracted as temporary geometry from an Alignment.
/// </remarks>
public class CivilAlignmentEntityWrapper : ICivilAlignmentEntity
{
    /// <inheritdoc />
    public string EntityType { get; }

    /// <inheritdoc />
    public double StartStation { get; }

    /// <inheritdoc />
    public double EndStation { get; }

    /// <inheritdoc />
    public double Length { get; }

    /// <inheritdoc />
    public int EntityIndex { get; }

    /// <inheritdoc />
    public Curve Curve { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilAlignmentEntityWrapper"/>.
    /// </summary>
    /// <param name="entityType">The type of entity (Line, Arc, Spiral, etc.).</param>
    /// <param name="startStation">The starting station along the alignment.</param>
    /// <param name="endStation">The ending station along the alignment.</param>
    /// <param name="length">The length of this entity.</param>
    /// <param name="entityIndex">The index of this entity in the alignment's entity collection.</param>
    /// <param name="curve">The geometry as a Rhino curve.</param>
    public CivilAlignmentEntityWrapper(
        string entityType,
        double startStation,
        double endStation,
        double length,
        int entityIndex,
        Curve curve)
    {
        EntityType = entityType;
        StartStation = startStation;
        EndStation = endStation;
        Length = length;
        EntityIndex = entityIndex;
        Curve = curve;
    }

    /// <summary>
    /// Creates a duplicate of this alignment entity wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilAlignmentEntityWrapper Duplicate()
    {
        var curveCopy = Curve.DuplicateCurve();
        return new CivilAlignmentEntityWrapper(
            EntityType,
            StartStation,
            EndStation,
            Length,
            EntityIndex,
            curveCopy);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Alignment Entity [{EntityType}] (Sta: {StartStation:F2} - {EndStation:F2}, Length: {Length:F2})";
    }
}
