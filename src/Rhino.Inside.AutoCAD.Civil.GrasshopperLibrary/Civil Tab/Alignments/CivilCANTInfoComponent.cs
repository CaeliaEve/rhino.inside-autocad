using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts values from Civil 3D Alignment CANT information.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilCANTInfoComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("B1C2D3E4-F5A6-7890-1234-567890ABCDE3");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilCANTInfoComponent"/> class.
    /// </summary>
    public CivilCANTInfoComponent()
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

        pManager.AddTextParameter("Critical Station Types", "CritType",
            "Types of CANT critical stations.", GH_ParamAccess.list);

        pManager.AddNumberParameter("Critical CANT Values", "CritCant",
            "CANT values at critical stations.", GH_ParamAccess.list);

        pManager.AddNumberParameter("Curve Start Stations", "CurveSta",
            "Start stations of CANT curves.", GH_ParamAccess.list);

        pManager.AddNumberParameter("Curve Radii", "CurveR",
            "Radii of CANT curves.", GH_ParamAccess.list);

        pManager.AddNumberParameter("Applied CANT", "AppCant",
            "Applied CANT values for curves.", GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        CivilCANTInfo? cantInfo = null;

        if (!DA.GetData(0, ref cantInfo) || cantInfo is null) return;

        DA.SetData(0, cantInfo.HasCANT);

        // Critical stations
        var critStations = cantInfo.CriticalStations.Select(c => c.Station).ToList();
        var critTypes = cantInfo.CriticalStations.Select(c => c.StationType).ToList();
        var critCants = cantInfo.CriticalStations.Select(c => c.Cant).ToList();

        DA.SetDataList(1, critStations);
        DA.SetDataList(2, critTypes);
        DA.SetDataList(3, critCants);

        // Curves
        var curveStarts = cantInfo.Curves.Select(c => c.StartStation).ToList();
        var curveRadii = cantInfo.Curves.Select(c => c.Radius).ToList();
        var appliedCants = cantInfo.Curves.Select(c => c.AppliedCant).ToList();

        DA.SetDataList(4, curveStarts);
        DA.SetDataList(5, curveRadii);
        DA.SetDataList(6, appliedCants);
    }
}
