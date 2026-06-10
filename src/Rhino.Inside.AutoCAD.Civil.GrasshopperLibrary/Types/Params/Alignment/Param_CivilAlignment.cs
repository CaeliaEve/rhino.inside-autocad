using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CivilAlignment = Autodesk.Civil.DatabaseServices.Alignment;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Alignments.
/// </summary>
public class Param_CivilAlignment : Param_AutocadObjectBase<GH_CivilAlignment, CivilAlignment>
{
    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("640c0e65-0982-4afa-bb2f-b3e8e022e66d");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Civil.GrasshopperLibrary.Properties.Resources.Param_CivilAlignment;

    /// <inheritdoc />
    protected override string SingularPromptMessage => "Select a Civil3d Alignment";

    /// <inheritdoc />
    protected override string PluralPromptMessage => "Select Civil3d Alignments";

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignment"/> class.
    /// </summary>
    public Param_CivilAlignment()
        : base("Civil3d Alignment", "CVL-Align",
            "A Civil 3D Alignment", "Params", "Civil3d")
    { }

    /// <inheritdoc />
    protected override IObjectFilter CreateSelectionFilter() => new CivilAlignmentFilter();

    /// <inheritdoc />
    protected override GH_CivilAlignment WrapEntity(CivilAlignment entity)
    {
        return new GH_CivilAlignment(entity);
    }
}
