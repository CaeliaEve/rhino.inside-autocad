using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using RhinoArc = Rhino.Geometry.Arc;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps a circular arc entity extracted from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// This wrapper provides access to circular arc-specific properties like radius
/// and center point, in addition to the base entity properties.
/// </remarks>
public class CivilProfileCircularArcWrapper : CivilProfileEntityWrapper, ICivilProfileCircularArc
{
    /// <inheritdoc />
    public double Radius { get; }

    /// <inheritdoc />
    public Point3d CenterPoint { get; }

    /// <inheritdoc />
    public Arc Arc { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfileCircularArcWrapper"/>.
    /// </summary>
    /// <param name="startStation">The starting station along the profile.</param>
    /// <param name="endStation">The ending station along the profile.</param>
    /// <param name="startElevation">The elevation at the start of the arc.</param>
    /// <param name="endElevation">The elevation at the end of the arc.</param>
    /// <param name="length">The length of this arc.</param>
    /// <param name="entityIndex">The index of this entity in the profile's entity collection.</param>
    /// <param name="curve">The geometry as a Rhino curve.</param>
    public CivilProfileCircularArcWrapper(
        double startStation,
        double endStation,
        double startElevation,
        double endElevation,
        double length,
        int entityIndex,
        RhinoArc arc,
        Curve curve)
        : base("CircularArc", startStation, endStation, startElevation, endElevation, length, entityIndex, curve)
    {
        this.Radius = arc.Radius;
        this.CenterPoint = arc.Center;
        this.Arc = arc;
    }

    /// <summary>
    /// Creates a duplicate of this profile circular arc wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public new CivilProfileCircularArcWrapper Duplicate()
    {
        var curveCopy = this.Curve.DuplicateCurve();
        return new CivilProfileCircularArcWrapper(
            this.StartStation,
            this.EndStation,
            this.StartElevation,
            this.EndElevation,
            this.Length,
            this.EntityIndex,
            this.Arc,
            curveCopy);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Profile Circular Arc (Sta: {this.StartStation:F2} - {this.EndStation:F2}, Radius: {this.Radius:F2})";
    }
}
