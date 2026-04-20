using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps a parabola (vertical curve) entity extracted from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// This wrapper provides access to parabola-specific properties like K value,
/// PVI station and elevation, and high/low point, in addition to the base entity properties.
/// </remarks>
public class CivilProfileAsymmetricalParabolaWrapper : CivilProfileEntityWrapper, ICivilProfileAsymmetricalParabola
{
    private readonly ProfileParabolaAsymmetric _parabola;
    /// <inheritdoc />
    public double PVIStation { get; }

    /// <inheritdoc />
    public double PVIElevation { get; }

    /// <inheritdoc />
    public double HighLowPointStation { get; }

    /// <inheritdoc />
    public double HighLowPointElevation { get; }

    /// <inheritdoc />
    public double GradeOut { get; }

    /// <inheritdoc />
    public double GradeIn { get; }

    /// <inheritdoc />
    public double AsymmetricLength1 { get; }

    /// <inheritdoc />
    public double AsymmetricLength2 { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfileAsymmetricalParabolaWrapper"/>.
    /// </summary>
    /// <param name="parabola"></param>
    /// <param name="entityIndex">The index of this entity in the profile's entity collection.</param>
    public CivilProfileAsymmetricalParabolaWrapper(ProfileParabolaAsymmetric parabola, int entityIndex)
        : base(parabola, entityIndex)
    {
        _parabola = parabola;

        this.PVIStation = parabola.PVIStation;
        this.PVIElevation = parabola.PVIElevation;
        this.HighLowPointStation = parabola.HighLowPointStation;
        this.HighLowPointElevation = parabola.HighLowPointElevation;
        this.GradeIn = parabola.GradeIn;
        this.GradeOut = parabola.GradeOut;
        this.AsymmetricLength1 = parabola.AsymmetricLength1;
        this.AsymmetricLength2 = parabola.AsymmetricLength2;

    }

    /// <summary>
    /// Calculates the elevation at a given station along a parabolic vertical curve.
    /// </summary>
    public double CalculateElevation(double station, double kValue)
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
        var correction = x * x / (2.0 * kValue * 100.0);

        // For sag curves, add correction; for crest curves, subtract
        // The sign depends on the relationship between gradeIn and gradeOut
        return tangentElevation - correction;
    }

    /// <summary>
    /// Creates a duplicate of this profile parabola wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public override CivilProfileAsymmetricalParabolaWrapper ShallowClone()
    {
        return new CivilProfileAsymmetricalParabolaWrapper(_parabola, this.EntityIndex);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Profile Parabola (Sta: {this.StartStation:F2} - {this.EndStation:F2}, PVI: {this.PVIStation:F2} @ {this.PVIElevation:F2})";
    }
}
