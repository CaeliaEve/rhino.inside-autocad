using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using CivilProfileView = Autodesk.Civil.DatabaseServices.ProfileView;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D ProfileViews.
/// </summary>
public class Param_CivilProfileView : Param_AutocadObjectBase<GH_CivilProfileView, CivilProfileView>
{
    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("A1B2C3D4-E5F6-7890-AB12-CD34EF567890");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilProfileView;

    /// <inheritdoc />
    protected override string SingularPromptMessage => "Select a Civil3d ProfileView";

    /// <inheritdoc />
    protected override string PluralPromptMessage => "Select Civil3d ProfileViews";

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileView"/> class.
    /// </summary>
    public Param_CivilProfileView()
        : base("Civil3d ProfileView", "CVL-PV",
            "A Civil 3D ProfileView", "Params", "Civil3d")
    { }

    /// <inheritdoc />
    protected override IObjectFilter CreateSelectionFilter() => new CivilProfileViewFilter();

    /// <inheritdoc />
    protected override GH_CivilProfileView WrapEntity(CivilProfileView entity)
    {
        return new GH_CivilProfileView(entity);
    }
}
