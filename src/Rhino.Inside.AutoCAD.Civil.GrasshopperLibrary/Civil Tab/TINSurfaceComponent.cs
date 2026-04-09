using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using CivilSurface = Autodesk.Civil.DatabaseServices.TinSurface;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts information from an AutoCAD DBObject.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class TINSurfaceComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("423C06AC-6A36-4F90-AC7C-BA42E12BBCE6");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance example.
    /// </summary>
    public TINSurfaceComponent()
        : base("Civil3d TIN Surface", "CVL-Surface",
            "A TIN Civil 3D surface",
            "Civil3d", "Example")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilTinSurface(), "Surface",
            "Srf", "A Civil3d Surface", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Id", "Id",
            "The Id of the Surface.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "StyleId", "StyleId",
            "The Id of the Style of the Surface.", GH_ParamAccess.item);

    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        CivilSurface? tinSurface = null;

        if (!DA.GetData(0, ref tinSurface) || tinSurface is null) return;

        // Id
        var id = new GH_AutocadObjectId(new AutocadObjectIdWrapper(tinSurface.Id));

        var styleId = new GH_AutocadObjectId(new AutocadObjectIdWrapper(tinSurface.StyleId));

        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var methods = typeof(CivilSurface)
            .GetMethods(System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Instance)
            .Select(m => m.Name)
            .Where(name => name.ToLower().Contains("contour") ||
                           name.ToLower().Contains("extract"))
            .Distinct()
            .OrderBy(n => n);

        foreach (var m in methods)
            RhinoApp.WriteLine(m);



        DA.SetData(0, id);
        DA.SetData(1, styleId);

    }
}
