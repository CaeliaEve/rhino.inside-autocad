using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that returns the AutoCAD Block Table Record which matches the name.
/// </summary>
[ComponentVersion(introduced: "1.2.24", updated: "1.2.29")]
public class GetAutocadBlockTableRecordByNameComponent : Block_BaseComponent
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("985c3037-a150-485f-ab6e-39693b10303c");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon =>
        Properties.Resources.GetAutocadBlockTableRecordByNameComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAutocadBlockTableRecordByNameComponent"/> class.
    /// </summary>
    public GetAutocadBlockTableRecordByNameComponent()
        : base("Get AutoCAD Block Table Record By Name", "AC-BlockByName",
            "Returns the AutoCAD Block Table Record which matches the name",
            "AutoCAD", "Blocks")
    {
        this.EnableStaleTracking();
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadDocument(GH_ParamAccess.item), "Document",
            "Doc",
            "An AutoCAD Document. If not provided, the active document will be used.",
            GH_ParamAccess.item);
        pManager[0].Optional = true;
        pManager.AddTextParameter("Name", "N",
            "The name of the AutoCAD Block Table Record to retrieve", GH_ParamAccess.item,
            string.Empty);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadBlockTableRecord(GH_ParamAccess.item),
            "BlockTableRecord", "BlkRec",
            "The AutoCAD Block Table Record matching the name",
            GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        AutocadDocument? autocadDocument = null;
        var name = string.Empty;

        DA.GetData(0, ref autocadDocument);

        if (autocadDocument is null)
        {
            var activeDoc = RhinoInsideAutoCadExtension.Application?.RhinoInsideManager?.AutoCadInstance?.ActiveDocument;
            if (activeDoc is null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No active AutoCAD document available");
                return;
            }
            autocadDocument = activeDoc as AutocadDocument;
        }

        if (autocadDocument is null)
            return;

        DA.GetData(1, ref name);

        var transactionManager = autocadDocument.CreateTransactionManager();

        _ = transactionManager.PerformTask(() =>
        {
            if (this.TryGetByName(transactionManager, name, out var blockTableRecord) == false)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"No Block Table Record exists with name: {name}");
                return false;
            }

            var gooBlockTableRecord = new GH_AutocadBlockTableRecord(blockTableRecord);

            DA.SetData(0, gooBlockTableRecord);

            return true;
        });
    }
}
