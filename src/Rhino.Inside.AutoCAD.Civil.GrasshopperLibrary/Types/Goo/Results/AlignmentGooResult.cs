using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents the result of extracting data from a Civil 3D Alignment.
/// </summary>
/// <param name="PropertiesGoo">The properties of the Alignment.</param>
/// <param name="EntitiesGoo">The individual entities (Lines, Arcs, Spirals) of the Alignment.</param>
/// <param name="Curve">The alignment centerline as a Rhino curve.</param>
/// <param name="LabelGroupsGoo">Auto-generated label groups from the Alignment.</param>
/// <param name="LabelsGoo">Individual labels from the Alignment.</param>
public record AlignmentGooResult(
    GH_CivilAlignmentProperties? PropertiesGoo,
    List<GH_CivilAlignmentEntity>? EntitiesGoo,
    Curve? Curve,
    List<GH_CivilAlignmentLabelGroup>? LabelGroupsGoo,
    List<GH_CivilFeatureLabel>? LabelsGoo) : GooResultBase
{
    /// <summary>
    /// Gets a failed result instance.
    /// </summary>
    public static AlignmentGooResult Failed => new(null, null, null, null, null) { IsSuccess = false };
}
