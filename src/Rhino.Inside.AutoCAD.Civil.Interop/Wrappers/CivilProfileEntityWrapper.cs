using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps an individual entity (segment) extracted from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted profile entity information.
/// Unlike other Civil 3D wrappers, this does not wrap a database object since
/// profile entities are extracted as temporary geometry from a Profile.
/// </remarks>
public class CivilProfileEntityWrapper : ICivilProfileEntity
{
    /// <inheritdoc />
    public string EntityType { get; }

    /// <inheritdoc />
    public double StartStation { get; }

    /// <inheritdoc />
    public double EndStation { get; }

    /// <inheritdoc />
    public double StartElevation { get; }

    /// <inheritdoc />
    public double EndElevation { get; }

    /// <inheritdoc />
    public double Length { get; }

    /// <inheritdoc />
    public int EntityIndex { get; }

    /// <inheritdoc />
    public Curve Curve { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfileEntityWrapper"/>.
    /// </summary>
    /// <param name="entityType">The type of entity (Tangent, CircularArc, Parabola, etc.).</param>
    /// <param name="startStation">The starting station along the profile.</param>
    /// <param name="endStation">The ending station along the profile.</param>
    /// <param name="startElevation">The elevation at the start of the entity.</param>
    /// <param name="endElevation">The elevation at the end of the entity.</param>
    /// <param name="length">The length of this entity.</param>
    /// <param name="entityIndex">The index of this entity in the profile's entity collection.</param>
    /// <param name="curve">The geometry as a Rhino curve.</param>
    public CivilProfileEntityWrapper(
        string entityType,
        double startStation,
        double endStation,
        double startElevation,
        double endElevation,
        double length,
        int entityIndex,
        Curve curve)
    {
        EntityType = entityType;
        StartStation = startStation;
        EndStation = endStation;
        StartElevation = startElevation;
        EndElevation = endElevation;
        Length = length;
        EntityIndex = entityIndex;
        Curve = curve;
    }

    /// <summary>
    /// Creates a duplicate of this profile entity wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilProfileEntityWrapper Duplicate()
    {
        var curveCopy = Curve.DuplicateCurve();
        return new CivilProfileEntityWrapper(
            EntityType,
            StartStation,
            EndStation,
            StartElevation,
            EndElevation,
            Length,
            EntityIndex,
            curveCopy);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Profile Entity [{EntityType}] (Sta: {StartStation:F2} - {EndStation:F2}, Elev: {StartElevation:F2} - {EndElevation:F2})";
    }
}
