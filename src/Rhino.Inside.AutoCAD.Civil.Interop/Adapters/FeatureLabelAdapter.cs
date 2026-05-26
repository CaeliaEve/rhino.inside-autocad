using Autodesk.AutoCAD.DatabaseServices;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Adapter holding text entities for a Civil 3D feature label, allowing
/// them to be transformed and morphed within the Rhino environment.
/// </summary>
public class FeatureLabelAdapter : IRhinoAdapter
{
    /// <summary>
    /// The list of text entities that make up the feature label. These are converted
    /// </summary>
    public List<TextEntity> TextEntities { get; }

    public FeatureLabelAdapter(IList<MText> cadTextEntities)
    {
        this.TextEntities = cadTextEntities.Select(entity => entity.ToRhinoTextEntity()).ToList();
    }

    private FeatureLabelAdapter(List<TextEntity> rhinoTextEntities)
    {
        this.TextEntities = rhinoTextEntities;
    }

    /// <inheritdoc />
    public BoundingBox GetBoundingBox()
    {
        var box = BoundingBox.Empty;

        foreach (var textEntity in this.TextEntities)
        {
            var textBoundingBox = textEntity.GetBoundingBox(false);
            box.Union(textBoundingBox);
        }
        return box;
    }

    /// <inheritdoc />
    public void Transform(Transform xform)
    {
        foreach (var textEntity in this.TextEntities)
        {
            textEntity.Transform(xform);
        }
    }

    /// <inheritdoc />
    public void Morph(SpaceMorph morph)
    {
        foreach (var textEntity in this.TextEntities)
        {
            morph.Morph(textEntity);
        }
    }

    /// <inheritdoc />
    public IRhinoAdapter Duplicate() => new FeatureLabelAdapter
    (
        this.TextEntities.Select(textEntity => textEntity.Duplicate() as TextEntity).ToList()
    );
}
