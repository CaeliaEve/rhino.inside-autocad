using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Adapter holding a collection of curves, allowing them to be
/// transformed and morphed within the Rhino environment.
/// </summary>
public class RhinoCurveCollectionAdapter : IRhinoAdapter
{
    /// <summary>
    /// Gets the collection of curves.
    /// </summary>
    public List<Curve> Curves { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RhinoCurveCollectionAdapter"/> class.
    /// </summary>
    /// <param name="curves">The curves to wrap.</param>
    public RhinoCurveCollectionAdapter(IEnumerable<Curve> curves)
    {
        Curves = curves?.ToList() ?? [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RhinoCurveCollectionAdapter"/> class
    /// from an existing list (used for duplication).
    /// </summary>
    private RhinoCurveCollectionAdapter(List<Curve> curves)
    {
        Curves = curves;
    }

    /// <inheritdoc />
    public BoundingBox GetBoundingBox()
    {
        var box = BoundingBox.Empty;

        foreach (var curve in Curves)
        {
            var curveBoundingBox = curve.GetBoundingBox(false);
            box.Union(curveBoundingBox);
        }

        return box;
    }

    /// <inheritdoc />
    public void Transform(Transform xform)
    {
        foreach (var curve in Curves)
        {
            curve.Transform(xform);
        }
    }

    /// <inheritdoc />
    public void Morph(SpaceMorph morph)
    {
        foreach (var curve in Curves)
        {
            morph.Morph(curve);
        }
    }

    /// <inheritdoc />
    public IRhinoAdapter Duplicate() => new RhinoCurveCollectionAdapter
    (
        Curves.Select(curve => curve.DuplicateCurve()).ToList()
    );
}
