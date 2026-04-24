using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents the result of extracting data from a Civil 3D Alignment.
/// </summary>
public class AlignmentGooResult : GooResultBase
{
    /// <summary>
    /// Gets the properties of the Alignment.
    /// </summary>
    public GH_CivilAlignmentProperties? PropertiesGoo { get; }

    /// <summary>
    /// Gets the individual entities (Lines, Arcs, Spirals) of the Alignment.
    /// </summary>
    public List<GH_CivilAlignmentEntity>? EntitiesGoo { get; }

    /// <summary>
    /// Gets the alignment centerline as a Rhino curve.
    /// </summary>
    public Curve? Curve { get; }

    /// <summary>
    /// Gets the auto-generated label groups from the Alignment.
    /// </summary>
    public List<GH_CivilAlignmentLabelGroup>? LabelGroupsGoo { get; }

    /// <summary>
    /// Gets the individual labels from the Alignment.
    /// </summary>
    public List<GH_CivilFeatureLabel>? LabelsGoo { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlignmentGooResult"/> class.
    /// </summary>
    /// <param name="propertiesGoo">The properties of the Alignment.</param>
    /// <param name="entitiesGoo">The individual entities (Lines, Arcs, Spirals) of the Alignment.</param>
    /// <param name="curve">The alignment centerline as a Rhino curve.</param>
    /// <param name="labelGroupsGoo">Auto-generated label groups from the Alignment.</param>
    /// <param name="labelsGoo">Individual labels from the Alignment.</param>
    public AlignmentGooResult(
        GH_CivilAlignmentProperties? propertiesGoo,
        List<GH_CivilAlignmentEntity>? entitiesGoo,
        Curve? curve,
        List<GH_CivilAlignmentLabelGroup>? labelGroupsGoo,
        List<GH_CivilFeatureLabel>? labelsGoo)
    {
        PropertiesGoo = propertiesGoo;
        EntitiesGoo = entitiesGoo;
        Curve = curve;
        LabelGroupsGoo = labelGroupsGoo;
        LabelsGoo = labelsGoo;
    }

    /// <summary>
    /// Gets a failed result instance.
    /// </summary>
    public static AlignmentGooResult Failed => new(null, null, null, null, null) { IsSuccess = false };
}
