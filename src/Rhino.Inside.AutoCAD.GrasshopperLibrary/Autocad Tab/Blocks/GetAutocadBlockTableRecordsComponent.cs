using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Host;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that returns the AutoCAD BlockTableRecords currently open in the AutoCAD session.
/// </summary>
[ComponentVersion(introduced: "1.0.0", updated: "1.3.0")]
public class GetAutocadBlockTableRecordsComponent : Block_BaseComponent
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("feb2beb6-7414-43e5-941a-d50f26a57ab7");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon =>
        Properties.Resources.GetAutocadBlockTableRecordsComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAutocadBlockTableRecordsComponent"/> class.
    /// </summary>
    public GetAutocadBlockTableRecordsComponent()
        : base("Get AutoCAD Block Table Records", "AC-BlkRecs",
            "Returns the list of all the AutoCAD Bloc1k Table Records in the document",
            "AutoCAD", "Blocks")
    {
        this.EnableStaleTracking();
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadDocument(GH_ParamAccess.item), "Document",
            "Doc", "An AutoCAD Document. If not provided, the active document will be used.", GH_ParamAccess.item);
        pManager[0].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadBlockTableRecord(GH_ParamAccess.list),
            "BlockTableRecords", "BlockTableRecords", "The AutoCAD BlockTableRecords",
            GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        AutocadDocument? autocadDocument = null;
        DA.GetData(0, ref autocadDocument);

        if (autocadDocument is null)
        {
            var activeDoc = AutoCadHostContext.ActiveDocument as AutocadDocument;
            if (activeDoc is null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No active AutoCAD document available");
                return;
            }
            autocadDocument = activeDoc as AutocadDocument;
        }

        if (autocadDocument is null)
            return;

        var transactionManager = autocadDocument.CreateTransactionManager();

        var blockTableRecordsRegister =
            transactionManager.PerformTask(() => this.GetAllRecords(transactionManager));

        var gooBlockTableRecords = blockTableRecordsRegister
            .Select(autocadLayerTableRecord =>
                new GH_AutocadBlockTableRecord(autocadLayerTableRecord))
            .ToList();

        DA.SetDataList(0, gooBlockTableRecords);
    }
}