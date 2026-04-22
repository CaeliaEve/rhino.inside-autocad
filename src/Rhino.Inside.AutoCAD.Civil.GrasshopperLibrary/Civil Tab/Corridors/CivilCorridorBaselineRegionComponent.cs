using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts information from a Civil 3D Corridor Baseline Region.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilCorridorBaselineRegionComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("F2A3B4C5-D6E7-8901-2345-678901234DEF");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilCorridorBaselineRegionComponent;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilCorridorBaselineRegionComponent"/> class.
    /// </summary>
    public CivilCorridorBaselineRegionComponent()
        : base("Civil3d Corridor Baseline Region", "CVL-CorrRgn",
            "Extracts information from a Civil 3D Corridor Baseline Region",
            "Civil3d", "Corridors")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilCorridorBaselineRegion(GH_ParamAccess.item), "Region",
            "Rgn", "A Corridor baseline region", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "N",
            "The name of the baseline region.", GH_ParamAccess.item);

        pManager.AddTextParameter("Assembly Name", "AsmName",
            "The name of the assembly applied to this region.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Assembly Id", "AsmId",
            "The Id of the assembly applied to this region.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Start Station", "StaSt",
            "The starting station of the region.", GH_ParamAccess.item);

        pManager.AddNumberParameter("End Station", "StaEnd",
            "The ending station of the region.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Length", "Len",
            "The length of the region.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilCorridorBaselineRegion? regionGoo = null;

        if (!DA.GetData(0, ref regionGoo) || regionGoo?.Value is null) return;

        var region = regionGoo.Value;

        var document = this.GetDocumentForObjectId(region.AssemblyId);
        if (document is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No document available");
            return;
        }

        var transactionManager = document.CreateTransactionManager();

        var assemblyName = transactionManager.PerformTask(() =>
        {
            var assembly = transactionManager.Unwrap()
                .GetObject(region.AssemblyId.Unwrap(), OpenMode.ForRead) as Assembly;

            return assembly.Name;
        });

        DA.SetData(0, region.Name);
        DA.SetData(1, assemblyName);
        DA.SetData(2, new GH_AutocadObjectId(region.AssemblyId));
        DA.SetData(3, region.StartStation);
        DA.SetData(4, region.EndStation);
        DA.SetData(5, region.Length);
    }
}
