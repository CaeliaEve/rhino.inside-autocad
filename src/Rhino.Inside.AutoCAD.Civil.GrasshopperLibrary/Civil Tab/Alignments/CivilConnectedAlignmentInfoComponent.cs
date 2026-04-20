using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts values from Civil 3D connected alignment information.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilConnectedAlignmentInfoComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("B1C2D3E4-F5A6-7890-1234-567890ABCDE4");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilConnectedAlignmentInfoComponent"/> class.
    /// </summary>
    public CivilConnectedAlignmentInfoComponent()
        : base("Civil3d Connected Alignment Info", "CVL-ConnAlign",
            "Extracts values from Civil 3D connected alignment information",
            "Civil3d", "Alignments")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilConnectedAlignmentInfo(GH_ParamAccess.item), "Connected Info",
            "ConnInfo", "Connected alignment information from a Civil3d Alignment", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddBooleanParameter("Is Connected", "Conn",
            "Whether the alignment is connected to other alignments.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_NamedId(GH_ParamAccess.item), "Parent Alignment",
            "Parent", "The parent alignment as a NamedId.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_NamedId(GH_ParamAccess.list), "Child Alignments",
            "Children", "Child alignments as NamedIds.", GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        CivilConnectedAlignmentInfo? connInfo = null;

        if (!DA.GetData(0, ref connInfo) || connInfo is null) return;

        DA.SetData(0, connInfo.IsConnected);
        DA.SetData(1, new GH_NamedId(connInfo.ParentAlignment as NamedId));

        var children = connInfo.ChildAlignments
            .Select(c => new GH_NamedId(c as NamedId))
            .ToList();
        DA.SetDataList(2, children);
    }
}
