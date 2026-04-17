using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that gets all Baselines from a Civil 3D Corridor.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class GetBaselinesFromCorridorComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("D6E7F8A9-B0C1-2345-6789-012345678123");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBaselinesFromCorridorComponent"/> class.
    /// </summary>
    public GetBaselinesFromCorridorComponent()
        : base("Get Baselines", "CVL-GetBLs",
            "Gets all Baselines from a Civil 3D Corridor",
            "Civil3d", "Corridors")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilCorridor(), "Corridor",
            "Corr", "A Civil3d Corridor to get baselines from", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilCorridorBaseline(GH_ParamAccess.list), "Baselines", "BLs",
            "The Baselines in this Corridor.", GH_ParamAccess.list);

        pManager.AddParameter(new Param_CivilCorridorBaselineRegion(GH_ParamAccess.list), "Regions", "Rgns",
            "All Baseline Regions from all Baselines.", GH_ParamAccess.list);

        pManager.AddParameter(new Param_CivilCorridorFeatureLine(GH_ParamAccess.list), "Feature Lines", "FLs",
            "All Feature Lines from all Baselines.", GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilCorridor? corridorGoo = null;

        if (!DA.GetData(0, ref corridorGoo) || corridorGoo is null) return;

        var corridorId = corridorGoo.Reference.ObjectId;

        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var transactionManager = document.CreateTransactionManager();

        var result = transactionManager.PerformTask(() =>
        {
            var corridor = transactionManager.Unwrap()
                .GetObject(corridorId.Unwrap(), OpenMode.ForRead) as Corridor;

            if (corridor == null)
                return (Baselines: new List<GH_CivilCorridorBaseline>(),
                        Regions: new List<GH_CivilCorridorBaselineRegion>(),
                        FeatureLines: new List<GH_CivilCorridorFeatureLine>());

            var baselines = new List<GH_CivilCorridorBaseline>();
            var allRegions = new List<GH_CivilCorridorBaselineRegion>();
            var allFeatureLines = new List<GH_CivilCorridorFeatureLine>();

            foreach (Baseline baseline in corridor.Baselines)
            {
                baselines.Add(new GH_CivilCorridorBaseline(new CivilCorridorBaselineWrapper(baseline)));

                // Get regions from this baseline
                var regions = baseline.GetRegions(transactionManager);
                allRegions.AddRange(regions.Select(r => new GH_CivilCorridorBaselineRegion(r)));

                // Get feature lines from this baseline
                var featureLines = baseline.GetFeatureLines(transactionManager);
                allFeatureLines.AddRange(featureLines.Select(fl => new GH_CivilCorridorFeatureLine(fl)));
            }

            return (Baselines: baselines, Regions: allRegions, FeatureLines: allFeatureLines);
        });

        if (result.Baselines.Count == 0)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No baselines found in this corridor");
            return;
        }

        DA.SetDataList(0, result.Baselines);
        DA.SetDataList(1, result.Regions);
        DA.SetDataList(2, result.FeatureLines);
    }
}
