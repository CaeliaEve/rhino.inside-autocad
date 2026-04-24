using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts values from Civil 3D connected alignment information.
/// </summary>
[ComponentVersion(introduced: "1.1.19")]
public class CivilConnectedAlignmentInfoComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("B1C2D3E4-F5A6-7890-1234-567890ABCDE4");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilConnectedAlignmentInfoComponent;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.quarternary;

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
        pManager.AddBooleanParameter("Is Connected Alignment", "IsConn",
            "Whether this is a connected alignment.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Connection Overlap Length In", "OverlapIn",
            "The connection overlap length at the incoming end.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Connection Overlap Length Out", "OverlapOut",
            "The connection overlap length at the outgoing end.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Incoming Parent Alignment Id",
            "InParentId", "The incoming parent alignment ObjectId.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Outgoing Parent Alignment Id",
            "OutParentId", "The outgoing parent alignment ObjectId.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Offset In", "OfsIn",
            "The offset value at the incoming connection.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Offset Out", "OfsOut",
            "The offset value at the outgoing connection.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        CivilConnectedAlignmentInfo? connInfo = null;

        if (!DA.GetData(0, ref connInfo) || connInfo is null) return;

        DA.SetData(0, connInfo.IsConnectedAlignment);
        DA.SetData(1, connInfo.ConnectionOverlapLengthIn);
        DA.SetData(2, connInfo.ConnectionOverlapLengthOut);
        DA.SetData(3, new GH_AutocadObjectId(connInfo.IncomingParentAlignmentId));
        DA.SetData(4, new GH_AutocadObjectId(connInfo.OutgoingParentAlignmentId));
        DA.SetData(5, connInfo.OffsetIn);
        DA.SetData(6, connInfo.OffsetOut);
    }
}
