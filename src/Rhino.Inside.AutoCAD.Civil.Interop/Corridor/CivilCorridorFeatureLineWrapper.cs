using Autodesk.Civil.DatabaseServices;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps a feature line extracted from a Civil 3D Corridor baseline.
/// </summary>
/// <remarks>
/// This wrapper captures feature line data including the geometry as a Rhino curve.
/// </remarks>
public class CivilCorridorFeatureLineWrapper : AutocadWrapperBase<CorridorFeatureLine>, ICivilCorridorFeatureLine
{
    /// <inheritdoc />
    public string Code { get; }

    /// <inheritdoc />
    public Curve Curve { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilCorridorFeatureLineWrapper"/>
    /// from a Civil 3D CorridorFeatureLine.
    /// </summary>
    /// <param name="featureLine">The corridor feature line to wrap.</param>
    /// <param name="code">The point code of the feature line.</param>
    /// <param name="curve">The Rhino curve geometry.</param>
    public CivilCorridorFeatureLineWrapper(CorridorFeatureLine featureLine, string code, Curve curve)
        : base(featureLine)
    {
        this.Code = code;
        this.Curve = curve;
    }

    /// <summary>
    /// Creates a duplicate of this feature line wrapper.
    /// </summary>
    /// <returns>A new instance with the same wrapped object and duplicated curve.</returns>
    public CivilCorridorFeatureLineWrapper ShallowClone()
    {
        return new CivilCorridorFeatureLineWrapper(
            _wrappedAutocadObject,
            this.Code,
            this.Curve.DuplicateCurve());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Feature Line: {this.Code}";
    }
}
