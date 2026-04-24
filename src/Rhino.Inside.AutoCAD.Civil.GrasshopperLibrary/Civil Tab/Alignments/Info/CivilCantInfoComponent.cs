using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts values from Civil 3D Alignment CANT information.
/// </summary>
[ComponentVersion(introduced: "1.1.19")]
public class CivilCantInfoComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("B1C2D3E4-F5A6-7890-1234-567890ABCDE3");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilCantInfoComponent;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.quarternary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilCantInfoComponent"/> class.
    /// </summary>
    public CivilCantInfoComponent()
        : base("Civil3d CANT Info", "CVL-CANT",
            "Extracts values from Civil 3D Alignment CANT (superelevation) information",
            "Civil3d", "Alignments")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilCANTInfo(GH_ParamAccess.item), "CANT Info",
            "CANT", "CANT information from a Civil3d Alignment", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddBooleanParameter("Has CANT", "Has",
            "Whether the alignment has CANT data.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Critical Stations", "CritSta",
            "Station values of CANT critical points.", GH_ParamAccess.list);

        pManager.AddNumberParameter("Critical CANT Values", "CritCant",
            "CANT values at critical stations.", GH_ParamAccess.list);

        pManager.AddNumberParameter("Curve Start Stations", "CurvSta",
            "Start stations of CANT curves.", GH_ParamAccess.list);

        pManager.AddNumberParameter("Curve End Stations", "CurveEnd",
            "End stations of CANT curves.", GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        CivilCantInfo? cantInfo = null;

        if (!DA.GetData(0, ref cantInfo) || cantInfo is null) return;

        DA.SetData(0, cantInfo.HasCantInfo);

        // Critical stations
        var critStations = cantInfo.CriticalStations.Select(c => c.Station).ToList();
        var critTypes = cantInfo.CriticalStations.Select(c => c.StationType).ToList();

        DA.SetDataList(1, critStations);
        DA.SetDataList(2, critTypes);

        // Curves
        var curveStarts = cantInfo.Curves.Select(c => c.StartStation).ToList();
        var CurveEnds = cantInfo.Curves.Select(c => c.EndStation).ToList();


        DA.SetDataList(3, curveStarts);
        DA.SetDataList(4, CurveEnds);

    }
}
