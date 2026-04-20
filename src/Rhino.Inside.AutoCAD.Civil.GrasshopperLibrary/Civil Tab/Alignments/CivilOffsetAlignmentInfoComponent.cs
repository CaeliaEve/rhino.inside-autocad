using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

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

        pManager.AddParameter(new Param_NamedId(GH_ParamAccess.item), "Parent Alignment",
            "Parent", "The parent alignment as a NamedId.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Nominal Offset", "Offset",
            "The nominal offset distance from the parent alignment.", GH_ParamAccess.item);

        pManager.AddTextParameter("Offset Side", "Side",
            "The side of the offset (Left or Right).", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        CivilOffsetAlignmentInfo? offsetInfo = null;

        if (!DA.GetData(0, ref offsetInfo) || offsetInfo is null) return;

        DA.SetData(0, offsetInfo.IsOffsetAlignment);
        DA.SetData(1, new GH_NamedId(offsetInfo.ParentAlignment as NamedId));
        DA.SetData(2, offsetInfo.NominalOffset);
        DA.SetData(3, offsetInfo.OffsetSide);
    }
}
