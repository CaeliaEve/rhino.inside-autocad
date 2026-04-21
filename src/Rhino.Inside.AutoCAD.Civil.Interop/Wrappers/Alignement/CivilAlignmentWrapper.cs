using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps a Civil 3D Alignment entity, providing access to its properties,
/// geometry, entities, and labels.
/// </summary>
/// <remarks>
/// This wrapper extracts all alignment data at construction time, creating
/// a snapshot of the alignment state. The wrapper requires an active transaction
/// to extract label groups and individual labels.
/// </remarks>
public class CivilAlignmentWrapper : AutocadEntityWrapper, ICivilAlignment
{
    private readonly Alignment _alignment;

    /// <inheritdoc />
    public IObjectId StyleId { get; }

    /// <inheritdoc />
    public ICivilAlignmentProperties Properties { get; }

    /// <inheritdoc />
    public List<ICivilAlignmentEntity> Entities { get; }

    /// <inheritdoc />
    public RhinoCurve? CenterlineCurve { get; }

    /// <inheritdoc />
    public List<ICivilAlignmentLabelGroup> LabelGroups { get; }

    /// <inheritdoc />
    public List<ICivilFeatureLabel> Labels { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilAlignmentWrapper"/>.
    /// </summary>
    /// <param name="alignment">The Civil 3D Alignment to wrap.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    public CivilAlignmentWrapper(Alignment alignment, IAutocadTransactionManager transactionManager)
        : base(alignment)
    {
        _alignment = alignment;

        this.StyleId = new AutocadObjectIdWrapper(alignment.StyleId);
        this.Properties = new CivilAlignmentProperties(alignment);
        this.Entities = this.ExtractEntities(alignment);
        this.CenterlineCurve = alignment.ToRhinoCurve();
        this.LabelGroups = this.ExtractLabelGroups(alignment, transactionManager);
        this.Labels = this.ExtractLabels(alignment, transactionManager);
    }

    /// <summary>
    /// Extracts all geometric entities from the alignment.
    /// </summary>
    private List<ICivilAlignmentEntity> ExtractEntities(Alignment alignment)
    {
        var entities = new List<ICivilAlignmentEntity>();
        var entityCollection = alignment.Entities;

        for (var i = 0; i < entityCollection.Count; i++)
        {
            var entity = entityCollection[i];

            var wrapper = new CivilAlignmentEntityWrapper(entity, i);

            entities.Add(wrapper);
        }

        return entities;

    }

    /// <summary>
    /// Extracts all label groups from the alignment.
    /// </summary>
    private List<ICivilAlignmentLabelGroup> ExtractLabelGroups(
        Alignment alignment,
        IAutocadTransactionManager transactionManager)
    {
        var labelGroups = new List<ICivilAlignmentLabelGroup>();

        try
        {
            var labelGroupIds = alignment.GetAlignmentLabelGroupIds();

            foreach (ObjectId labelGroupId in labelGroupIds)
            {
                if (labelGroupId.IsNull || labelGroupId.IsErased)
                    continue;

                var labelGroup = transactionManager.Unwrap()
                    .GetObject(labelGroupId, OpenMode.ForRead) as AlignmentLabelGroup;

                if (labelGroup == null)
                    continue;

                var wrapper = new CivilAlignmentLabelGroupWrapper(labelGroup);
                labelGroups.Add(wrapper);
            }
        }
        catch
        {
            // Return empty list if label group extraction fails
        }

        return labelGroups;
    }

    /// <summary>
    /// Extracts all individual labels from the alignment.
    /// </summary>
    private List<ICivilFeatureLabel> ExtractLabels(
        Alignment alignment,
        IAutocadTransactionManager transactionManager)
    {
        var labels = new List<ICivilFeatureLabel>();

        try
        {
            var labelIds = alignment.GetAlignmentLabelIds();

            foreach (ObjectId labelId in labelIds)
            {
                if (labelId.IsNull || labelId.IsErased)
                    continue;

                var featureLabel = transactionManager.Unwrap()
                    .GetObject(labelId, OpenMode.ForRead) as FeatureLabel;

                if (featureLabel == null)
                    continue;

                var wrapper = featureLabel.CreateLabelWrapper(transactionManager);
                if (wrapper != null)
                {
                    labels.Add(wrapper);
                }
            }
        }
        catch
        {
            // Return empty list if label extraction fails
        }

        return labels;
    }

    /// <inheritdoc />
    public override IDbObject ShallowClone()
    {
        throw new InvalidOperationException(
            "CivilAlignmentWrapper requires a transaction manager and cannot be shallow cloned. " +
            "Create a new instance with the original Alignment object instead.");
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Alignment: {this.Properties.Name} (Length: {this.Properties.Length:F2}, Entities: {this.Entities.Count})";
    }
}
