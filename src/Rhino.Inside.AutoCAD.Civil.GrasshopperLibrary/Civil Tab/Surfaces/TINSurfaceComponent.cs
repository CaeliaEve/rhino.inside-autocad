using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

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

        pManager.AddParameter(new Param_CivilTinProperties(), "TIN Properties", "TP",
            "Surface statistics (use TIN Properties component to extract values).", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilSurfaceBoundary(GH_ParamAccess.list), "Boundaries", "B",
            "The boundary definitions of the Surface.", GH_ParamAccess.list);

        pManager.AddParameter(new Param_CivilSurfaceContour(GH_ParamAccess.list), "Contours", "C",
            "The contour lines of the Surface.", GH_ParamAccess.list);

        pManager.AddParameter(new Param_CivilSurfaceBreakline(GH_ParamAccess.list), "Breaklines", "BL",
            "The breakline definitions of the Surface.", GH_ParamAccess.list);

        pManager.AddMeshParameter("Mesh", "M",
            "The surface as a Rhino mesh.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilTinSurface? tinSurfaceGoo = null;

        if (!DA.GetData(0, ref tinSurfaceGoo) || tinSurfaceGoo is null) return;

        var surfaceId = tinSurfaceGoo.Reference.ObjectId;

        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var transactionManager = document.CreateTransactionManager();

        var tinSurface = transactionManager.PerformTask(() =>
            transactionManager.Unwrap().GetObject(surfaceId.Unwrap(), OpenMode.ForRead) as
            TinSurface);

        if (tinSurface == null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to read TIN Surface");
            return;
        }

        // Id
        var id = new GH_AutocadObjectId(surfaceId);
        DA.SetData(0, id);

        // StyleId
        var styleId = new GH_AutocadObjectId(new AutocadObjectIdWrapper(tinSurface.StyleId));
        DA.SetData(1, styleId);

        // TIN Properties
        var tinPropsWrapper = new CivilTinPropertiesWrapper(tinSurface);
        DA.SetData(2, new GH_CivilTinProperties(tinPropsWrapper));

        // Surface data
        var surfaceData = transactionManager.PerformTask(() => new
        {
            Boundaries = tinSurface.GetBoundaries(transactionManager),
            Contours = tinSurface.GetContours(transactionManager),
            Breaklines = tinSurface.GetBreaklines(transactionManager),
            Mesh = tinSurface.ToRhinoMesh(transactionManager)
        });

        var boundaryGooList = surfaceData.Boundaries
            .Select(b => new GH_CivilSurfaceBoundary(b))
            .ToList();

        var contourGooList = surfaceData.Contours
            .Select(c => new GH_CivilSurfaceContour(c))
            .ToList();

        var breaklineGooList = surfaceData.Breaklines
            .Select(bl => new GH_CivilSurfaceBreakline(bl))
            .ToList();

        DA.SetDataList(3, boundaryGooList);
        DA.SetDataList(4, contourGooList);
        DA.SetDataList(5, breaklineGooList);

        // Mesh
        DA.SetData(6, surfaceData.Mesh);
    }
}
