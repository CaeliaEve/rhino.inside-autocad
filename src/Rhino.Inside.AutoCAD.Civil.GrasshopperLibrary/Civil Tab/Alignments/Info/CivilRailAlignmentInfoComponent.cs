using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts values from Civil 3D rail alignment information.
/// </summary>
[ComponentVersion(introduced: "1.1.19")]
public class CivilRailAlignmentInfoComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("B1C2D3E4-F5A6-7890-1234-567890ABCDE6");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.quarternary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilRailAlignmentInfoComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilRailAlignmentInfoComponent"/> class.
    /// </summary>
    public CivilRailAlignmentInfoComponent()
        : base("Civil3d Rail Alignment Info", "CVL-RailAlign",
            "Extracts values from Civil 3D rail alignment information",
            "Civil3d", "Alignments")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilRailAlignmentInfo(GH_ParamAccess.item), "Rail Info",
            "RailInfo", "Rail alignment information from a Civil3d Alignment", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddBooleanParameter("Is Rail Alignment", "IsRail",
            "Whether this is a rail alignment.", GH_ParamAccess.item);

        pManager.AddNumberParameter("TrackWidth", "Width",
            "The rail gauge value.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        CivilRailAlignmentInfo? railInfo = null;

        if (!DA.GetData(0, ref railInfo) || railInfo is null) return;

        DA.SetData(0, railInfo.IsRailAlignment);
        DA.SetData(1, railInfo.TrackWidth);
    }
}
