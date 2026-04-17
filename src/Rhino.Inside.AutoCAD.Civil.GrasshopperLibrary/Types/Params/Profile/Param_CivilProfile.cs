using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CivilProfile = Autodesk.Civil.DatabaseServices.Profile;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Profiles.
/// </summary>
public class Param_CivilProfile : Param_AutocadObjectBase<GH_CivilProfile, CivilProfile>
{
    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("D1E2F3A4-B5C6-7890-DEF1-234567890ABC");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Civil.GrasshopperLibrary.Properties.Resources.CivilDefault;

    /// <inheritdoc />
    protected override string SingularPromptMessage => "Select a Civil3d Profile";

    /// <inheritdoc />
    protected override string PluralPromptMessage => "Select Civil3d Profiles";

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfile"/> class.
    /// </summary>
    public Param_CivilProfile()
        : base("Civil3d Profile", "CVL-Profile",
            "A Civil 3D Profile", "Params", "Civil3d")
    { }

    /// <inheritdoc />
    protected override IObjectFilter CreateSelectionFilter() => new CivilProfileFilter();

    /// <inheritdoc />
    protected override GH_CivilProfile WrapEntity(CivilProfile entity)
    {
        return new GH_CivilProfile(entity);
    }
}
