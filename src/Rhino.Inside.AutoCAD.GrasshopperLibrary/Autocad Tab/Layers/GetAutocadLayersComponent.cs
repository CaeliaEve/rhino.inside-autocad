using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that returns the AutoCAD layers currently open in the AutoCAD session.
/// </summary>
[ComponentVersion(introduced: "1.0.0", updated: "1.3.0")]
public class GetAutocadLayersComponent : Layer_BaseComponent
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("41c4ed14-3a97-4812-94bc-4950bca8be7d");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.GetAutocadLayersComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAutocadLayersComponent"/> class.
    /// </summary>
    public GetAutocadLayersComponent()
        : base("Get AutoCAD Layers", "AC-Lyrs",
            "Returns the list of all the AutoCAD layer in the document",
            "AutoCAD", "Layers")
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
        pManager.AddParameter(new Param_AutocadLayer(GH_ParamAccess.list), "Layers", "Layers", "The AutoCAD Layers",
            GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        AutocadDocument? autocadDocument = null;
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

        var transactionManager = autocadDocument.CreateTransactionManager();

        _ = transactionManager.PerformTask(() =>
        {
            var layersRegister = this.GetAllRecords(transactionManager);

            var gooLayers = layersRegister
                .Select(layer => new GH_AutocadLayer(layer))
                .ToList();

            DA.SetDataList(0, gooLayers);

            return true;
        });

    }
}
