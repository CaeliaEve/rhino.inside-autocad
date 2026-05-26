using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts values from Civil 3D Alignment reference station.
/// </summary>
[ComponentVersion(introduced: "1.1.19")]
public class CivilReferenceStationComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("B1C2D3E4-F5A6-7890-1234-567890ABCDE1");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilReferenceStationComponent;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.quarternary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilReferenceStationComponent"/> class.
    /// </summary>
    public CivilReferenceStationComponent()
        : base("Civil3d Reference Station", "CVL-RefSta",
            "Extracts values from Civil 3D Alignment reference station information",
            "Civil3d", "Alignments")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilReferenceStation(GH_ParamAccess.item), "Reference Station",
            "RefSta", "Reference station information from a Civil3d Alignment", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddBooleanParameter("Has Reference Point", "Has",
            "Whether the alignment has a reference point.", GH_ParamAccess.item);

        pManager.AddPointParameter("Reference Point", "Pt",
            "The reference point location in world coordinates.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Reference Point Station", "Sta",
            "The station value at the reference point.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        CivilReferenceStation? refStation = null;

        if (!DA.GetData(0, ref refStation) || refStation is null) return;

        DA.SetData(0, refStation.HasReferencePoint);
        DA.SetData(1, refStation.ReferencePoint);
        DA.SetData(2, refStation.ReferencePointStation);
    }
}
