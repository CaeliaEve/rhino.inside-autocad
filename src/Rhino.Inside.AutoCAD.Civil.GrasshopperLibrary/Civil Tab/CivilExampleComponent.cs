using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts information from an AutoCAD DBObject.
/// </summary>
[ComponentVersion(introduced: "1.2.17")]
public class CivilExampleComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("7157045A-A626-4DEF-BDDF-58F99A9BFD29");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance example.
    /// </summary>
    public CivilExampleComponent()
        : base("Civil Example", "CVL-Example",
            "Civil 3d Example",
            "Civil3d", "Example")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadObject(GH_ParamAccess.item), "DBObject",
            "Obj", "An AutoCAD DBObject", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddBooleanParameter("Test", "Test", "Test", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        AutocadDbObjectWrapper? dbObject = null;

        if (!DA.GetData(0, ref dbObject) || dbObject is null) return;

        // Id
        var id = dbObject.Id;

        DA.SetData(0, true);

    }
}
