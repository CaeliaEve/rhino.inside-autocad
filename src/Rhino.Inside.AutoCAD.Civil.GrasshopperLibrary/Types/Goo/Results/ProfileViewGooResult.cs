using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents the result of extracting data from a Civil 3D ProfileView.
/// </summary>
/// <param name="PropertiesGoo">The properties of the ProfileView, such as its name and coordinate system.</param>
/// <param name="ProfileDataGoo">A list of profiles displayed in the ProfileView, represented as geometric data.</param>
/// <param name="AlignmentGoo">The parent alignment associated with the ProfileView.</param>
/// <param name="BandsGoo">A list of bands (top and bottom) of the ProfileView, containing information such as band type and style.</param>
/// <param name="LabelGroupsGoo">A list of labels in the ProfileView.</param>
public record ProfileViewGooResult(
    GH_CivilProfileViewProperties? PropertiesGoo,
    List<GH_CivilProfile>? ProfileDataGoo,
    GH_CivilAlignment? AlignmentGoo,
    List<GH_CivilProfileViewBand>? BandsGoo,
    List<GH_CivilFeatureLabel>? LabelGroupsGoo,
    GH_Structure<IGH_GeometricGoo> Geoemtry) : GooResultBase
{
    /// <summary>
    /// Gets a failed result instance.
    /// </summary>
    public static ProfileViewGooResult Failed => new(null, null, null, null, null, null) { IsSuccess = false };
}
