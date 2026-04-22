using Autodesk.AutoCAD.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that sets properties for an AutoCAD layout.
/// </summary>
[ComponentVersion(introduced: "1.0.0", updated: "1.0.5")]
public class SetAutocadLayoutComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("e7f9a1b4-8c0d-5e3f-9a2b-8d6c5f8e9b3f");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.SetAutocadLayoutInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetAutocadLayoutComponent"/> class.
    /// </summary>
    public SetAutocadLayoutComponent()
        : base("Set AutoCAD Layout", "AC-SetLay",
            "Sets properties for an AutoCAD Layout",
            "AutoCAD", "Layouts")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadLayout(GH_ParamAccess.item), "Layout",
            "Layout", "An AutoCAD Layout", GH_ParamAccess.item);

        pManager.AddTextParameter("NewName", "Name",
            "New layout name", GH_ParamAccess.item);

        // Make all parameters optional except the first
        pManager[1].Optional = true;

    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "Name",
            "The name of the AutoCAD Layout", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Id", "Id",
            "The Id of the AutoCAD Layout", GH_ParamAccess.item);

        pManager.AddIntegerParameter("TabOrder", "TabOrder",
            "The tab display order", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "BlockTableRecordId", "BlockId",
            "The associated block table record ID", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        AutocadLayoutWrapper? layout = null;

        if (!DA.GetData(0, ref layout) || layout is null) return;

        var newName = layout.Name;

        DA.GetData(1, ref newName);

        if (string.IsNullOrWhiteSpace(newName))
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Layout name cannot be empty");
            return;
        }

        var change = newName != layout.Name;

        if (change)
        {
            var document = this.GetDocumentForObjectId(layout.Id);
            if (document is null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No document available");
                return;
            }

            var transactionManager = document.CreateTransactionManager();

            layout = transactionManager.PerformTask(() =>
            {
                var transaction = transactionManager.Unwrap();

                var cadLayout =
                    transaction.GetObject(layout.Id.Unwrap(), OpenMode.ForWrite) as Layout;

                cadLayout!.LayoutName = newName;

                return new AutocadLayoutWrapper(cadLayout);
            });

            // Renaming a layout does not trigger a modified event
            document.LayoutRegister.Update();
        }

        // Output updated values
        DA.SetData(0, layout.Name);
        DA.SetData(1, layout.Id);
        DA.SetData(2, layout.TabOrder);
        DA.SetData(3, layout.BlockTableRecordId);
    }
}
