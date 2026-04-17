using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps a tangent (straight line) entity extracted from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// This wrapper provides access to tangent-specific properties like grade,
/// in addition to the base entity properties.
/// </remarks>
public class CivilProfileTangentWrapper : CivilProfileEntityWrapper, ICivilProfileTangent
{
    /// <inheritdoc />
    public double Grade { get; }

    /// <inheritdoc />
    public Line Line { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfileTangentWrapper"/>.
    /// </summary>
    /// <param name="startStation">The starting station along the profile.</param>
    /// <param name="endStation">The ending station along the profile.</param>
    /// <param name="startElevation">The elevation at the start of the tangent.</param>
    /// <param name="endElevation">The elevation at the end of the tangent.</param>
    /// <param name="length">The length of this tangent.</param>
    /// <param name="entityIndex">The index of this entity in the profile's entity collection.</param>
    /// <param name="grade">The grade (slope) of this tangent as a percentage.</param>
    /// <param name="line">The geometry as a Rhino line.</param>
    /// <param name="curve">The geometry as a Rhino curve.</param>
    public CivilProfileTangentWrapper(
        double startStation,
        double endStation,
        double startElevation,
        double endElevation,
        double length,
        int entityIndex,
        double grade,
        Line line,
        Curve curve)
        : base("Tangent", startStation, endStation, startElevation, endElevation, length, entityIndex, curve)
    {
        Grade = grade;
        Line = line;
    }

    /// <summary>
    /// Creates a duplicate of this profile tangent wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public new CivilProfileTangentWrapper Duplicate()
    {
        var curveCopy = Curve.DuplicateCurve();
        return new CivilProfileTangentWrapper(
            StartStation,
            EndStation,
            StartElevation,
            EndElevation,
            Length,
            EntityIndex,
            Grade,
            Line,
            curveCopy);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Profile Tangent (Sta: {StartStation:F2} - {EndStation:F2}, Grade: {Grade:F2}%)";
    }
}
