using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents the result of extracting data from a Civil 3D Profile.
/// </summary>
/// <param name="PropertiesGoo">The properties of the Profile.</param>
/// <param name="EntitiesGoo">The individual entities (Tangents, CircularArcs, Parabolas) of the Profile.</param>
/// <param name="Curve">The profile as a 2D Rhino curve (X=Station, Y=Elevation).</param>
/// <param name="AlignmentGoo">The parent alignment of this profile.</param>
/// <param name="LabelGroupsGoo">Label groups from the Profile.</param>
public record ProfileGooResult(
    GH_CivilProfileProperties? PropertiesGoo,
    List<GH_CivilProfileEntity>? EntitiesGoo,
    Curve? Curve,
    GH_CivilAlignment? AlignmentGoo,
    List<GH_CivilProfileLabelGroup>? LabelGroupsGoo) : GooResultBase
{
    /// <summary>
    /// Gets a failed result instance.
    /// </summary>
    public static ProfileGooResult Failed => new(null, null, null, null, null) { IsSuccess = false };
}
