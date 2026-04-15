using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using TinSurface = Autodesk.Civil.DatabaseServices.TinSurface;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts information from a Civil 3D TIN Surface.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class TINSurfaceComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("423C06AC-6A36-4F90-AC7C-BA42E12BBCE6");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="TINSurfaceComponent"/> class.
    /// </summary>
    public TINSurfaceComponent()
        : base("Civil3d TIN Surface", "CVL-Surface",
            "Extracts information from a Civil 3D TIN surface",
            "Civil3d", "Surfaces")
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
        TinSurface? tinSurface = null;

        if (!DA.GetData(0, ref tinSurface) || tinSurface is null) return;

        // Id
        var id = new GH_AutocadObjectId(new AutocadObjectIdWrapper(tinSurface.Id));
        DA.SetData(0, id);

        // StyleId
        var styleId = new GH_AutocadObjectId(new AutocadObjectIdWrapper(tinSurface.StyleId));
        DA.SetData(1, styleId);
    }
}
