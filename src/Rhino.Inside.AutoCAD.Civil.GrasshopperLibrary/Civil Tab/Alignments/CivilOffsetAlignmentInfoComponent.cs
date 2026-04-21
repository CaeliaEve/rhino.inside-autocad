using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts values from Civil 3D offset alignment information.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilOffsetAlignmentInfoComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("B1C2D3E4-F5A6-7890-1234-567890ABCDE5");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.quarternary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilOffsetAlignmentInfoComponent"/> class.
    /// </summary>
    public CivilOffsetAlignmentInfoComponent()
        : base("Civil3d Offset Alignment Info", "CVL-OfsAlign",
            "Extracts values from Civil 3D offset alignment information",
            "Civil3d", "Alignments")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilOffsetAlignmentInfo(GH_ParamAccess.item), "Offset Info",
            "OfsInfo", "Offset alignment information from a Civil3d Alignment", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddBooleanParameter("Is Offset Alignment", "IsOfs",
            "Whether this is an offset alignment.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Nominal Offset", "Offset",
            "The nominal offset distance from the parent alignment.", GH_ParamAccess.item);

        pManager.AddTextParameter("Side", "Side",
            "The side of the offset (Left or Right).", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Parent Alignment Id",
            "ParentId", "The parent alignment ObjectId.", GH_ParamAccess.item);

        // Regions
        pManager.AddNumberParameter("Region Start Stations", "RegStaSt",
            "Start stations of offset regions.", GH_ParamAccess.list);

        pManager.AddNumberParameter("Region End Stations", "RegStaEnd",
            "End stations of offset regions.", GH_ParamAccess.list);

        pManager.AddNumberParameter("Region Offsets", "RegOfs",
            "Offset values of regions.", GH_ParamAccess.list);

    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        CivilOffsetAlignmentInfo? offsetInfo = null;

        if (!DA.GetData(0, ref offsetInfo) || offsetInfo is null) return;

        DA.SetData(0, offsetInfo.IsOffsetAlignment);
        DA.SetData(1, offsetInfo.NominalOffset);
        DA.SetData(2, offsetInfo.Side);
        DA.SetData(3, new GH_AutocadObjectId(offsetInfo.ParentAlignmentId));

        // Regions
        DA.SetDataList(4, offsetInfo.Regions.Select(r => r.StartStation).ToList());
        DA.SetDataList(5, offsetInfo.Regions.Select(r => r.EndStation).ToList());
        DA.SetDataList(6, offsetInfo.Regions.Select(r => r.Offset).ToList());

    }
}
