using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from a Civil 3D Alignment Label Group.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilAlignmentLabelGroupComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("A7B8C9D0-E1F2-3456-0123-67890ABCDEF1");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilAlignmentLabelGroupComponent"/> class.
    /// </summary>
    public CivilAlignmentLabelGroupComponent()
        : base("Civil3d Alignment Label Group", "CVL-AlignLblGrp",
            "Extracts individual values from a Civil 3D Alignment Label Group",
            "Civil3d", "Labels")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilAlignmentLabelGroup(GH_ParamAccess.item), "Label Group",
            "LG", "An alignment label group from a Civil3d Alignment", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Label Group Type", "Type",
            "The type of this label group (e.g., AlignmentStationLabelGroup).", GH_ParamAccess.item);

        pManager.AddTextParameter("Style Name", "Style",
            "The name of the label style applied to this group.", GH_ParamAccess.item);

        pManager.AddIntegerParameter("Label Count", "Count",
            "The number of sub-entity labels in this group.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilAlignmentLabelGroup? labelGroupGoo = null;

        if (!DA.GetData(0, ref labelGroupGoo) || labelGroupGoo?.Value is null) return;

        var labelGroup = labelGroupGoo.Value;

        DA.SetData(0, labelGroup.LabelGroupType);
        DA.SetData(1, labelGroup.StyleName);
        DA.SetData(2, labelGroup.LabelCount);
    }
}
