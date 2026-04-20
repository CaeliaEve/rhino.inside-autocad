using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts values from Civil 3D Alignment design speeds.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilDesignSpeedsComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("B1C2D3E4-F5A6-7890-1234-567890ABCDE2");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilDesignSpeedsComponent"/> class.
    /// </summary>
    public CivilDesignSpeedsComponent()
        : base("Civil3d Design Speeds", "CVL-DesSpd",
            "Extracts values from Civil 3D Alignment design speed information",
            "Civil3d", "Alignments")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilDesignSpeeds(GH_ParamAccess.item), "Design Speeds",
            "DesSpd", "Design speed information from a Civil3d Alignment", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddNumberParameter("Design Speed", "Speed",
            "The base design speed for the alignment.", GH_ParamAccess.item);

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

        DA.SetData(0, designSpeeds.DesignSpeed);

        var stations = designSpeeds.SpeedStations.Select(s => s.Station).ToList();
        var speeds = designSpeeds.SpeedStations.Select(s => s.Speed).ToList();

        DA.SetDataList(1, stations);
        DA.SetDataList(2, speeds);
    }
}
