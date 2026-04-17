using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Deconstructor component for Civil 3D Alignment Design Speed Label Groups.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilAlignmentDesignSpeedLabelGroupComponent : RhinoInsideAutocad_ComponentBase
{
    public override Guid ComponentGuid => new("A7B8C9D0-E1F2-3456-0123-67890ABCDE21");
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    public CivilAlignmentDesignSpeedLabelGroupComponent()
        : base("Civil3d Design Speed Label Group", "CVL-DesSpeedLblGrp",
            "Extracts values from an Alignment Design Speed Label Group. Non-matching types are skipped.",
            "Civil3d", "Labels")
    { }

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilAlignmentLabelGroup(GH_ParamAccess.item),
            "Label Group", "LG", "An alignment label group", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Label Group Type", "Type", "The type of this label group.", GH_ParamAccess.item);
        pManager.AddTextParameter("Style Name", "Style", "The label style name.", GH_ParamAccess.item);
        pManager.AddIntegerParameter("Label Count", "Count", "The number of labels in this group.", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilAlignmentLabelGroup? goo = null;
        if (!DA.GetData(0, ref goo) || goo?.Value == null) return;

        if (goo.Value is not CivilAlignmentDesignSpeedLabelGroupWrapper labelGroup)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                $"Skipping non-DesignSpeed label group: {goo.Value.LabelGroupType}");
            return;
        }

        DA.SetData(0, labelGroup.LabelGroupType);
        DA.SetData(1, labelGroup.StyleName);
        DA.SetData(2, labelGroup.LabelCount);
    }
}