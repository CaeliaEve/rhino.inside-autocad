using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts values from Civil 3D Alignment design speeds.
/// </summary>
[ComponentVersion(introduced: "1.1.19")]
public class CivilDesignSpeedsInfoComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("B1C2D3E4-F5A6-7890-1234-567890ABCDE2");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDesignSpeedsInfoComponent;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.quarternary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilDesignSpeedsInfoComponent"/> class.
    /// </summary>
    public CivilDesignSpeedsInfoComponent()
        : base("Civil3d Design Speeds Info", "CVL-Speed",
            "Extracts values from Civil 3D Alignment design speed information",
            "Civil3d", "Alignments")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilDesignSpeeds(GH_ParamAccess.item), "Design Speeds",
            "Speed", "Design speed information from a Civil3d Alignment", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddNumberParameter("Stations", "Sta",
            "Station values for speed changes.", GH_ParamAccess.list);

        pManager.AddNumberParameter("Speeds", "Spd",
            "Speed values at each station.", GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        CivilDesignSpeeds? designSpeeds = null;

        if (!DA.GetData(0, ref designSpeeds) || designSpeeds is null) return;

        var stations = designSpeeds.SpeedStations.Select(s => s.Station).ToList();
        var speeds = designSpeeds.SpeedStations.Select(s => s.Speed).ToList();

        DA.SetDataList(0, stations);
        DA.SetDataList(1, speeds);
    }
}
