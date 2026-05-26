using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Adapter holding a collection of curves and text entities, allowing them to be
/// transformed and morphed within the Rhino environment.
/// </summary>
public class RhinoGraphAdapter : IRhinoAdapter
{
    /// <summary>
    /// Gets the collection of curves.
    /// </summary>
    public List<Curve> Curves { get; }

    /// <summary>
    /// Gets the collection of text entities.
    /// </summary>
    public List<TextEntity> TextEntities { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RhinoGraphAdapter"/> class.
    /// </summary>
    /// <param name="curves">The curves to wrap.</param>
    /// <param name="textEntities">The text entities to wrap.</param>
    public RhinoGraphAdapter(IEnumerable<Curve>? curves, IEnumerable<TextEntity>? textEntities)
    {
        Curves = curves?.ToList() ?? [];
        TextEntities = textEntities?.ToList() ?? [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RhinoGraphAdapter"/> class
    /// from existing lists (used for duplication).
    /// </summary>
    private RhinoGraphAdapter(List<Curve> curves, List<TextEntity> textEntities)
    {
        Curves = curves;
        TextEntities = textEntities;
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

        foreach (var textEntity in TextEntities)
        {
            var textBoundingBox = textEntity.GetBoundingBox(false);
            box.Union(textBoundingBox);
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

        foreach (var textEntity in TextEntities)
        {
            textEntity.Transform(xform);
        }
    }

    /// <inheritdoc />
    public void Morph(SpaceMorph morph)
    {
        foreach (var curve in Curves)
        {
            morph.Morph(curve);
        }

        foreach (var textEntity in TextEntities)
        {
            morph.Morph(textEntity);
        }
    }

    /// <inheritdoc />
    public IRhinoAdapter Duplicate() => new RhinoGraphAdapter
    (
        Curves.Select(curve => curve.DuplicateCurve()).ToList(),
        TextEntities.Select(textEntity => textEntity.Duplicate() as TextEntity).ToList()
    );
}
