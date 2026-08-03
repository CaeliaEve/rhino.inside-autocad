using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that returns the AutoCAD layout which matches the name.
/// </summary>
[ComponentVersion(introduced: "1.0.0", updated: "1.3.0")]
public class GetAutocadLayoutByNameComponent : Layout_BaseComponent
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("d6e7b9c0-3e4f-5a8b-0c2d-9f6e5b8d0d3f");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.GetAutocadLayoutsByNameComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAutocadLayoutByNameComponent"/> class.
    /// </summary>
    public GetAutocadLayoutByNameComponent()
        : base("Get AutoCAD Layout By Name", "AC-Lay",
            "Returns the AutoCAD layout which matches the name",
            "AutoCAD", "Layouts")
    {
        this.EnableStaleTracking();
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadDocument(GH_ParamAccess.item), "Document",
            "Doc", "An AutoCAD Document. If not provided, the active document will be used.", GH_ParamAccess.item);
        pManager[0].Optional = true;

        pManager.AddTextParameter("Name", "N", "The name of the AutoCAD Layout to retrieve", GH_ParamAccess.item, string.Empty);

    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadLayout(GH_ParamAccess.item), "Layout", "Layout",
            "The AutoCAD Layout matching the name, or the default layout if no matching layout is found",
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

        var transactionManger = autocadDocument.CreateTransactionManager();

        _ = transactionManger.PerformTask(() =>
        {
            if (this.TryGetByName(transactionManger, name, out var layout) == false)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"No layout exists with name: {name}");
                return false;
            }

            var gooLayout = new GH_AutocadLayout(layout);

            DA.SetData(0, gooLayout);

            return true;
        });
    }
}
