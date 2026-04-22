using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts station and elevation from a Civil 3D Station Point.
/// </summary>
[ComponentVersion(introduced: "1.0.19")]
public class CivilStationPointComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("C3D4E5F6-A7B8-9012-CDEF-012345678904");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap? Icon => Properties.Resources.CivilStationPointComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilStationPointComponent"/> class.
    /// </summary>
    public CivilStationPointComponent()
        : base("Civil3d Station Point", "CVL-StaPt",
            "Extracts station and elevation from a Civil 3D Station Point",
            "Civil3d", "Profiles")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilStationPoint(GH_ParamAccess.item), "Station Point",
            "StaPt", "The station point to deconstruct", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddNumberParameter("Station", "Sta",
            "The station value along an alignment or profile.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Elevation", "Elev",
            "The elevation at this station.", GH_ParamAccess.item);

        pManager.AddPointParameter("Point", "Pt",
            "The station point as a Rhino Point3d (Station, Elevation, 0).", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilStationPoint? stationPointGoo = null;

        if (!DA.GetData(0, ref stationPointGoo) || stationPointGoo?.Value is null) return;

        var stationPoint = stationPointGoo.Value;

        DA.SetData(0, stationPoint.Station);
        DA.SetData(1, stationPoint.Elevation);
        DA.SetData(2, stationPoint.ToRhinoPoint3d());
    }
}
