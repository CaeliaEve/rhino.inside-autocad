using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that returns the AutoCAD layouts currently in the AutoCAD document.
/// </summary>
[ComponentVersion(introduced: "1.0.0", updated: "1.0.20")]
public class GetAutocadLayoutsComponent : Layout_BaseComponent
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("c5f6a8b9-2d3e-4f7a-9b1c-8e5d4a7f9c2e");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.GetAutocadLayoutsComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAutocadLayoutsComponent"/> class.
    /// </summary>
    public GetAutocadLayoutsComponent()
        : base("Get AutoCAD Layouts", "AC-Lays",
            "Returns the list of all the AutoCAD layouts in the document",
            "AutoCAD", "Layouts")
    {
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
        pManager.AddParameter(new Param_AutocadLayout(GH_ParamAccess.list), "Layouts", "Layouts", "The AutoCAD Layouts",
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
            var layoutsRegister = this.GetAllRecords(transactionManager);

            var gooLayouts = layoutsRegister
                .Select(layout => new GH_AutocadLayout(layout))
                .ToList();

            DA.SetDataList(0, gooLayouts);

            return true;
        });

    }
}
