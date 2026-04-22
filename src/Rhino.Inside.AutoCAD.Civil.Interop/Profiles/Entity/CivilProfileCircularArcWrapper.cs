using Autodesk.Civil.DatabaseServices;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using RhinoArc = Rhino.Geometry.Arc;
using RhinoArcCurve = Rhino.Geometry.ArcCurve;
using RhinoCurve = Rhino.Geometry.Curve;
using RhinoPoint3d = Rhino.Geometry.Point3d;

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
    private readonly ProfileCircular _profile;

    /// <inheritdoc />
    public double Radius { get; }

    /// <inheritdoc />
    public bool IsCrest { get; }

    /// <inheritdoc />
    public double HighLowPointStation { get; }

    /// <inheritdoc />
    public double HighLowPointElevation { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfileCircularArcWrapper"/>.
    /// </summary>
    /// <param name="profile">The civil3d profile.</param>
    /// <param name="entityIndex">The index of this entity in the profile's entity collection.</param>
    public CivilProfileCircularArcWrapper(
        ProfileCircular profile, int entityIndex)
        : base(profile, entityIndex)
    {
        _profile = profile;
        this.Radius = profile.Radius;

        this.IsCrest = profile.GradeIn > profile.GradeOut;

        this.HighLowPointStation = profile.HighLowPointStation;

        this.HighLowPointElevation = profile.HighLowPointElevation;
    }

    /// <summary>
    /// Creates a duplicate of this profile circular arc wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public override CivilProfileCircularArcWrapper ShallowClone()
    {
        return new CivilProfileCircularArcWrapper(_profile, this.EntityIndex);
    }

    /// <summary>
    /// Converts a ProfileCircular to a wrapper.
    /// </summary>
    public override RhinoCurve ToRhinoCurve()
    {
        var startPoint = this.Start.ToRhinoPoint3d();
        var endPoint = this.End.ToRhinoPoint3d();
        var radius = UnitConverter.ToRhinoLength(this.Radius);

        var isCrest = this.IsCrest;

        var centerStation = this.HighLowPointStation;
        var centerElevation = isCrest
            ? this.HighLowPointElevation - this.Radius
            : this.HighLowPointElevation + this.Radius;

        var centerPoint = new RhinoPoint3d(
            UnitConverter.ToRhinoLength(centerStation),
            UnitConverter.ToRhinoLength(centerElevation),
            0);

        var circle = new Rhino.Geometry.Circle(centerPoint, radius);

        if (!circle.IsValid)
        {
            throw new InvalidOperationException(
                "Failed to create a valid circle from the AlignmentArc points.");
        }

        _ = circle.ClosestParameter(startPoint, out var start);
        _ = circle.ClosestParameter(endPoint, out var end);

        var interval = new Interval(start, end);

        var rhinoArc = new RhinoArc(circle, interval);

        if (!rhinoArc.IsValid)
        {
            throw new InvalidOperationException(
                "Failed to create a valid Rhino arc from the AlignmentArc.");
        }

        var rhinoArcCurve = new RhinoArcCurve(rhinoArc);

        return rhinoArcCurve;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Profile Circular Arc (Start:[{this.Start}] - End:[{this.End}], Radius: {this.Radius:F2})";
    }
}
