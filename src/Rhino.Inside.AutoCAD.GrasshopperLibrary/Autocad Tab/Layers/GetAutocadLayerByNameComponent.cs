using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Host;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that returns the AutoCAD documents currently open in the AutoCAD session.
/// </summary>
[ComponentVersion(introduced: "1.0.0", updated: "1.3.0")]
public class GetAutocadLayerByNameComponent : Layer_BaseComponent
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("e74496d3-c465-4676-8584-c6f277bfbf0e");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon =>
        Properties.Resources.GetAutocadLayersByNameComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAutocadLayersComponent"/> class.
    /// </summary>
    public GetAutocadLayerByNameComponent()
        : base("Get AutoCAD Layer By Name", "AC-Lyr",
            "Returns the the AutoCAD layer which matches the name",
            "AutoCAD", "Layers")
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
            "The name of the AutoCAD Layer to retrieve", GH_ParamAccess.item,
            string.Empty);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadLayer(GH_ParamAccess.item), "Layer",
            "Layer",
            "The AutoCAD Layer matching the name, or the default layer if no matching layer is found",
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
            var activeDoc = (AutoCadHostContext.HostApplication as Rhino.Inside.AutoCAD.Core.Interfaces.IRhinoInsideAutoCadApplication)?.RhinoInsideManager
                ?.AutoCadInstance?.ActiveDocument;
            if (activeDoc is null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "No active AutoCAD document available");
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
            if (this.TryGetByName(transactionManager, name, out var layer) == false)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"No layer exists with name: {name}");
                return false;
            }

            var gooLayer = new GH_AutocadLayer(layer);

            DA.SetData(0, gooLayer);

            return true;
        });
    }
}
