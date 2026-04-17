using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps a parabola (vertical curve) entity extracted from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// This wrapper provides access to parabola-specific properties like K value,
/// PVI station and elevation, and high/low point, in addition to the base entity properties.
/// </remarks>
public class CivilProfileParabolaWrapper : CivilProfileEntityWrapper, ICivilProfileParabola
{
    /// <inheritdoc />
    public double KValue { get; }

    /// <inheritdoc />
    public double PVIStation { get; }

    /// <inheritdoc />
    public double PVIElevation { get; }

    /// <inheritdoc />
    public Point3d? HighLowPoint { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfileParabolaWrapper"/>.
    /// </summary>
    /// <param name="startStation">The starting station along the profile.</param>
    /// <param name="endStation">The ending station along the profile.</param>
    /// <param name="startElevation">The elevation at the start of the parabola.</param>
    /// <param name="endElevation">The elevation at the end of the parabola.</param>
    /// <param name="length">The length of this parabola.</param>
    /// <param name="entityIndex">The index of this entity in the profile's entity collection.</param>
    /// <param name="kValue">The K value (rate of change of grade).</param>
    /// <param name="pviStation">The station of the PVI.</param>
    /// <param name="pviElevation">The elevation of the PVI.</param>
    /// <param name="highLowPoint">The high or low point, if one exists within the curve.</param>
    /// <param name="curve">The geometry as a Rhino curve.</param>
    public CivilProfileParabolaWrapper(
        double startStation,
        double endStation,
        double startElevation,
        double endElevation,
        double length,
        int entityIndex,
        double kValue,
        double pviStation,
        double pviElevation,
        Point3d? highLowPoint,
        Curve curve)
        : base("Parabola", startStation, endStation, startElevation, endElevation, length, entityIndex, curve)
    {
        KValue = kValue;
        PVIStation = pviStation;
        PVIElevation = pviElevation;
        HighLowPoint = highLowPoint;
    }

    /// <summary>
    /// Creates a duplicate of this profile parabola wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public new CivilProfileParabolaWrapper Duplicate()
    {
        var curveCopy = Curve.DuplicateCurve();
        return new CivilProfileParabolaWrapper(
            StartStation,
            EndStation,
            StartElevation,
            EndElevation,
            Length,
            EntityIndex,
            KValue,
            PVIStation,
            PVIElevation,
            HighLowPoint,
            curveCopy);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Profile Parabola (Sta: {StartStation:F2} - {EndStation:F2}, K: {KValue:F2}, PVI: {PVIStation:F2} @ {PVIElevation:F2})";
    }
}
