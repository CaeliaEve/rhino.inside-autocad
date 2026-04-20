using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts information from a Civil 3D TIN Volume Surface.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class TINVolumeSurfaceComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("D5E7A3B1-8C4F-4D2E-9A6B-1F3C7E8D9A2B");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="TINVolumeSurfaceComponent"/> class.
    /// </summary>
    public TINVolumeSurfaceComponent()
        : base("Civil3d TIN Volume Surface", "CVL-VolSurface",
            "Extracts information from a Civil 3D TIN Volume surface including volume statistics",
            "Civil3d", "Surfaces")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilTinVolumeSurface(), "Volume Surface",
            "VolSrf", "A Civil3d Volume Surface", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Id", "Id",
            "The Id of the Volume Surface.", GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The name of the Volume Surface.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "StyleId", "StyleId",
            "The Id of the Style of the Volume Surface.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilVolumeProperties(GH_ParamAccess.item), "Volume Properties", "VP",
            "Volume statistics (use Volume Properties component to extract values).", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilTinSurface(), "Base Surface", "BS",
            "The base TIN surface.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilTinSurface(), "Comparison Surface", "CS",
            "The comparison TIN surface.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilTinVolumeSurface? volumeSurfaceGoo = null;

        if (!DA.GetData(0, ref volumeSurfaceGoo) || volumeSurfaceGoo is null) return;

        var surfaceId = volumeSurfaceGoo.Reference.ObjectId;

        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var transactionManager = document.CreateTransactionManager();

        var volumeSurface = transactionManager.PerformTask(() =>
            transactionManager.Unwrap().GetObject(surfaceId.Unwrap(), OpenMode.ForRead) as
            TinVolumeSurface);

        if (volumeSurface == null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to read Volume Surface");
            return;
        }

        // Id
        var id = new GH_AutocadObjectId(surfaceId);
        DA.SetData(0, id);

        // Name
        DA.SetData(1, volumeSurface.Name);

        // StyleId
        var styleId = new GH_AutocadObjectId(new AutocadObjectIdWrapper(volumeSurface.StyleId));
        DA.SetData(2, styleId);

        // Volume Properties
        var volumePropsWrapper = CivilTinVolumeSurfaceProperties.CreateFromVolume(volumeSurface);
        DA.SetData(3, new GH_CivilVolumeProperties(volumePropsWrapper));

        // Get the base and comparison surfaces
        var volumeProps = volumeSurface.GetVolumeProperties();
        var baseSurfaceId = volumeProps.BaseSurface;
        var comparisonSurfaceId = volumeProps.ComparisonSurface;

        var baseSurface = transactionManager.PerformTask(() =>
            transactionManager.Unwrap().GetObject(baseSurfaceId, OpenMode.ForRead) as TinSurface);

        var comparisonSurface = transactionManager.PerformTask(() =>
            transactionManager.Unwrap().GetObject(comparisonSurfaceId, OpenMode.ForRead) as TinSurface);

        // Base Surface
        if (baseSurface != null)
            DA.SetData(4, new GH_CivilTinSurface(baseSurface));

        // Comparison Surface
        if (comparisonSurface != null)
            DA.SetData(5, new GH_CivilTinSurface(comparisonSurface));
    }
}
