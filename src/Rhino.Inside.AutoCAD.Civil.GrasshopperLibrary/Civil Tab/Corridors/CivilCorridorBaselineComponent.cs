using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts information from a Civil 3D Corridor Baseline.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilCorridorBaselineComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("E1F2A3B4-C5D6-7890-1234-567890123CDE");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilCorridorBaselineComponent;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilCorridorBaselineComponent"/> class.
    /// </summary>
    public CivilCorridorBaselineComponent()
        : base("Civil3d Corridor Baseline", "CVL-CorrBL",
            "Extracts information from a Civil 3D Corridor Baseline",
            "Civil3d", "Corridors")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilCorridorBaseline(GH_ParamAccess.item), "Baseline",
            "BL", "A Corridor baseline", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "N",
            "The name of the baseline.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Alignment Id", "AlignId",
            "The Id of the alignment associated with this baseline.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Profile Id", "ProfId",
            "The Id of the profile associated with this baseline.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Start Station", "StaSt",
            "The starting station of the baseline.", GH_ParamAccess.item);

        pManager.AddNumberParameter("End Station", "StaEnd",
            "The ending station of the baseline.", GH_ParamAccess.item);

    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilCorridorBaseline? baselineGoo = null;

        if (!DA.GetData(0, ref baselineGoo) || baselineGoo?.Value is null) return;

        var baseline = baselineGoo.Value;

        DA.SetData(0, baseline.Name);
        DA.SetData(1, new GH_AutocadObjectId(baseline.AlignmentId));
        DA.SetData(2, new GH_AutocadObjectId(baseline.ProfileId));
        DA.SetData(3, baseline.StartStation);
        DA.SetData(4, baseline.EndStation);
    }
}
