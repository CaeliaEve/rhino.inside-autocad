using Autodesk.AutoCAD.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using CivilFeatureLabel = Autodesk.Civil.DatabaseServices.FeatureLabel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Grasshopper Goo for Civil 3D Feature labels (generic - can hold any label type).
/// </summary>
/// <remarks>
/// This Goo wraps a Civil 3D FeatureLabel and converts its text components to Rhino TextEntities
/// for display in both the Rhino viewport and AutoCAD preview.
/// </remarks>
public class GH_CivilFeatureLabel : GH_AutocadGeometricGoo<CivilFeatureLabel, FeatureLabelAdapter>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilFeatureLabel"/> class with no value.
    /// </summary>
    public GH_CivilFeatureLabel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilFeatureLabel"/> class with the
    /// specified Civil 3D Feature Label.
    /// </summary>
    /// <param name="label">The Civil 3D Feature Label to wrap.</param>
    public GH_CivilFeatureLabel(CivilFeatureLabel label) : base(label)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilFeatureLabel"/> class with the
    /// specified Civil 3D Feature Label.
    /// </summary>
    /// <param name="label">The Civil 3D Feature Label to wrap.</param>
    public GH_CivilFeatureLabel(ICivilFeatureLabel label) : base((CivilFeatureLabel)label.Unwrap())
    {
    }

    /// <summary>
    /// A private constructor used to create a reference Goo which is a clone of the
    /// input feature label.
    /// </summary>
    private GH_CivilFeatureLabel(CivilFeatureLabel label, IAutocadReferenceId referenceId) : base(label, referenceId)
    {
    }

    /// <inheritdoc />
    protected override GH_AutocadGeometricGoo<CivilFeatureLabel, FeatureLabelAdapter> CreateClonedInstance(CivilFeatureLabel entity)
    {
        return new GH_CivilFeatureLabel(entity.Clone() as CivilFeatureLabel, this.Reference);
    }

    /// <inheritdoc />
    protected override GH_AutocadGeometricGoo<CivilFeatureLabel, FeatureLabelAdapter> CreateInstance(CivilFeatureLabel entity)
    {
        return new GH_CivilFeatureLabel(entity);
    }

    /// <inheritdoc />
    protected override CivilFeatureLabel? Convert(FeatureLabelAdapter rhinoType)
    {
        // Converting from Rhino TextEntities back to Civil 3D Feature Label is not supported
        return null;
    }

    /// <inheritdoc />
    protected override FeatureLabelAdapter? Convert(CivilFeatureLabel wrapperType)
    {
        if (wrapperType == null)
            return null;

        var textComponentIds = wrapperType.GetTextComponentIds();
        var mTextEntities = new List<MText>();

        var database = wrapperType.Database;
        if (database == null)
            return null;

        using var transaction = database.TransactionManager.StartTransaction();

        foreach (ObjectId textCompId in textComponentIds)
        {
            if (textCompId.IsNull || textCompId.IsErased)
                continue;

            var dbObject = transaction.GetObject(textCompId, OpenMode.ForRead);

            if (dbObject is MText mText)
            {
                mTextEntities.Add(mText);
            }
            else if (dbObject is DBText dbText)
            {
                // Convert DBText to MText for consistency
                var convertedMText = ConvertDbTextToMText(dbText);
                mTextEntities.Add(convertedMText);
            }
        }

        transaction.Commit();

        return mTextEntities.Count > 0 ? new FeatureLabelAdapter(mTextEntities) : null;
    }

    /// <summary>
    /// Converts a DBText entity to an MText entity for consistent handling.
    /// </summary>
    private static MText ConvertDbTextToMText(DBText dbText)
    {
        var mText = new MText
        {
            Contents = dbText.TextString,
            Location = dbText.Position,
            TextHeight = dbText.Height,
            Rotation = dbText.Rotation,
            TextStyleId = dbText.TextStyleId,
            Layer = dbText.Layer,
            Color = dbText.Color,
            Width = 0
        };

        return mText;
    }

    /// <inheritdoc />
    protected override void DrawViewportGeometryWires(GH_PreviewWireArgs args)
    {
        var adapter = this.RhinoGeometry;
        if (adapter == null)
            return;

        foreach (var textEntity in adapter.TextEntities)
        {
            if (textEntity != null)
            {
                args.Pipeline.DrawText(textEntity, args.Color, textEntity.DimensionScale);
            }
        }
    }

    /// <inheritdoc />
    protected override void DrawViewportGeometryMeshes(GH_PreviewMeshArgs args)
    {
        // Feature labels are drawn as text/wires only
    }

    /// <inheritdoc />
    public override void DrawAutocadPreview(IGrasshopperPreviewData previewData)
    {
        var adapter = this.RhinoGeometry;
        if (adapter == null)
            return;

        foreach (var textEntity in adapter.TextEntities)
        {
            if (textEntity != null)
            {
                previewData.Texts.Add(textEntity);
            }
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Civil3d Feature Label";

        return $"Civil3d Feature Label [Type: {this.Value.GetType().Name}, Id: {this.Reference}]";
    }
}
