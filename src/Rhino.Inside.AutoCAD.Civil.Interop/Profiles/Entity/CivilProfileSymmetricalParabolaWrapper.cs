using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using RhinoCurve = Rhino.Geometry.Curve;
using RhinoNurbsCurve = Rhino.Geometry.NurbsCurve;
using RhinoPoint3d = Rhino.Geometry.Point3d;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps a parabola (vertical curve) entity extracted from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// This wrapper provides access to parabola-specific properties like K value,
/// PVI station and elevation, and high/low point, in addition to the base entity properties.
/// </remarks>
public class CivilProfileSymmetricalParabolaWrapper : CivilProfileEntityWrapper, ICivilProfileSymmetricalParabola
{
    private readonly ProfileParabolaSymmetric _parabola;

    /// <inheritdoc />
    public double KValue { get; }

    /// <inheritdoc />
    public double PVIStation { get; }

    /// <inheritdoc />
    public double PVIElevation { get; }

    /// <inheritdoc />
    public double HighLowPointStation { get; }

    /// <inheritdoc />
    public double HighLowPointElevation { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfileSymmetricalParabolaWrapper"/>.
    /// </summary>
    /// <param name="parabola"></param>
    /// <param name="entityIndex">The index of this entity in the profile's entity collection.</param>
    public CivilProfileSymmetricalParabolaWrapper(ProfileParabolaSymmetric parabola, int entityIndex)
        : base(parabola, entityIndex)
    {
        _parabola = parabola;
        this.KValue = parabola.K;
        this.PVIStation = parabola.PVIStation;
        this.PVIElevation = parabola.PVIElevation;
        this.HighLowPointStation = parabola.HighLowPointStation;
        this.HighLowPointElevation = parabola.HighLowPointElevation;

    }

    /// <summary>
    /// Calculates the elevation at a given station along a parabolic vertical curve.
    /// </summary>
    public double CalculateElevation(double station)
    {

        var gradeIn = _parabola.GradeIn;

        // Distance from PVI
        var x = station - this.PVIStation;

        // Tangent elevation at this station
        var tangentElevation = this.PVIElevation + gradeIn * x;

        // Parabolic correction
        // y = gradeIn * x + (gradeOut - gradeIn) * x^2 / (2 * L)
        // where L = K * |gradeOut - gradeIn|
        // Simplified: correction = x^2 / (2 * K * 100)
        var correction = x * x / (2.0 * this.KValue * 100.0);

        // For sag curves, add correction; for crest curves, subtract
        // The sign depends on the relationship between gradeIn and gradeOut
        return tangentElevation - correction;
    }

    /// <summary>
    /// Creates a duplicate of this profile parabola wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public override CivilProfileEntityWrapper ShallowClone()
    {
        return new CivilProfileSymmetricalParabolaWrapper(_parabola, this.EntityIndex);
    }

    /// <summary>
    /// Converts a ProfileParabolaSymmetric to a wrapper.
    /// </summary>
    public override RhinoCurve ToRhinoCurve()
    {
        var points = new List<RhinoPoint3d>();
        var numSamples = Math.Max(20, (int)(this.Length / 2.0));

        var startStation = this.Start.Station;
        var endStation = this.End.Station;

        // Sample points along the parabola
        for (var i = 0; i <= numSamples; i++)
        {
            var t = (double)i / numSamples;
            var station = startStation + (endStation - startStation) * t;

            // Calculate elevation using parabola equation
            var elevation = this.CalculateElevation(station);

            points.Add(new RhinoPoint3d(
                UnitConverter.ToRhinoLength(station),
                UnitConverter.ToRhinoLength(elevation),
                0));
        }

        return RhinoNurbsCurve.Create(false, 3, points);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Profile Parabola (Start:[{this.Start}] - End:[{this.End}], K: {this.KValue:F2}, PVI: {this.PVIStation:F2} @ {this.PVIElevation:F2})";
    }
}
