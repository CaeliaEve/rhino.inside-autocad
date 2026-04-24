using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents the result of extracting data from a Civil 3D ProfileView.
/// </summary>
public class ProfileViewGooResult : GooResultBase
{
    /// <summary>
    /// Gets the properties of the ProfileView, such as its name and coordinate system.
    /// </summary>
    public GH_CivilProfileViewProperties? PropertiesGoo { get; }

    /// <summary>
    /// Gets the list of profiles displayed in the ProfileView, represented as geometric data.
    /// </summary>
    public List<GH_CivilProfile>? ProfileDataGoo { get; }

    /// <summary>
    /// Gets the parent alignment associated with the ProfileView.
    /// </summary>
    public GH_CivilAlignment? AlignmentGoo { get; }

    /// <summary>
    /// Gets the list of bands (top and bottom) of the ProfileView, containing information such as band type and style.
    /// </summary>
    public List<GH_CivilProfileViewBand>? BandsGoo { get; }

    /// <summary>
    /// Gets the list of labels in the ProfileView.
    /// </summary>
    public List<GH_CivilFeatureLabel>? LabelGroupsGoo { get; }

    /// <summary>
    /// Gets the geometry associated with the ProfileView.
    /// </summary>
    public GH_Structure<IGH_GeometricGoo>? Geometry { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileViewGooResult"/> class.
    /// </summary>
    /// <param name="propertiesGoo">The properties of the ProfileView.</param>
    /// <param name="profileDataGoo">A list of profiles displayed in the ProfileView.</param>
    /// <param name="alignmentGoo">The parent alignment associated with the ProfileView.</param>
    /// <param name="bandsGoo">A list of bands of the ProfileView.</param>
    /// <param name="labelGroupsGoo">A list of labels in the ProfileView.</param>
    /// <param name="geometry">The geometry associated with the ProfileView.</param>
    public ProfileViewGooResult(
        GH_CivilProfileViewProperties? propertiesGoo,
        List<GH_CivilProfile>? profileDataGoo,
        GH_CivilAlignment? alignmentGoo,
        List<GH_CivilProfileViewBand>? bandsGoo,
        List<GH_CivilFeatureLabel>? labelGroupsGoo,
        GH_Structure<IGH_GeometricGoo>? geometry)
    {
        PropertiesGoo = propertiesGoo;
        ProfileDataGoo = profileDataGoo;
        AlignmentGoo = alignmentGoo;
        BandsGoo = bandsGoo;
        LabelGroupsGoo = labelGroupsGoo;
        Geometry = geometry;
    }

    /// <summary>
    /// Gets a failed result instance.
    /// </summary>
    public static ProfileViewGooResult Failed => new(null, null, null, null, null, null) { IsSuccess = false };
}
