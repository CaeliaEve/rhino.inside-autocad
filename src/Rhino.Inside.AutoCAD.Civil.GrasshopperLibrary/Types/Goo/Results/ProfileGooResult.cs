using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents the result of extracting data from a Civil 3D Profile.
/// </summary>
public class ProfileGooResult : GooResultBase
{
    /// <summary>
    /// Gets the properties of the Profile.
    /// </summary>
    public GH_CivilProfileProperties? PropertiesGoo { get; }

    /// <summary>
    /// Gets the individual entities (Tangents, CircularArcs, Parabolas) of the Profile.
    /// </summary>
    public List<GH_CivilProfileEntity>? EntitiesGoo { get; }

    /// <summary>
    /// Gets the profile as a 2D Rhino curve (X=Station, Y=Elevation).
    /// </summary>
    public Curve? Curve { get; }

    /// <summary>
    /// Gets the parent alignment of this profile.
    /// </summary>
    public GH_CivilAlignment? AlignmentGoo { get; }

    /// <summary>
    /// Gets the label groups from the Profile.
    /// </summary>
    public List<GH_CivilProfileLabelGroup>? LabelGroupsGoo { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileGooResult"/> class.
    /// </summary>
    /// <param name="propertiesGoo">The properties of the Profile.</param>
    /// <param name="entitiesGoo">The individual entities (Tangents, CircularArcs, Parabolas) of the Profile.</param>
    /// <param name="curve">The profile as a 2D Rhino curve (X=Station, Y=Elevation).</param>
    /// <param name="alignmentGoo">The parent alignment of this profile.</param>
    /// <param name="labelGroupsGoo">Label groups from the Profile.</param>
    public ProfileGooResult(
        GH_CivilProfileProperties? propertiesGoo,
        List<GH_CivilProfileEntity>? entitiesGoo,
        Curve? curve,
        GH_CivilAlignment? alignmentGoo,
        List<GH_CivilProfileLabelGroup>? labelGroupsGoo)
    {
        PropertiesGoo = propertiesGoo;
        EntitiesGoo = entitiesGoo;
        Curve = curve;
        AlignmentGoo = alignmentGoo;
        LabelGroupsGoo = labelGroupsGoo;
    }

    /// <summary>
    /// Gets a failed result instance.
    /// </summary>
    public static ProfileGooResult Failed => new(null, null, null, null, null) { IsSuccess = false };
}
