using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts information from a Civil 3D Parcel.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilParcelComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("F6A7B8C9-D0E1-2345-F012-678901234567");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilParcelComponent"/> class.
    /// </summary>
    public CivilParcelComponent()
        : base("Civil3d Parcel", "CVL-Parcel",
            "Extracts information from a Civil 3D Parcel",
            "Civil3d", "Parcels")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilParcel(), "Parcel",
            "P", "A Civil3d Parcel", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Id", "Id",
            "The Id of the Parcel.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "StyleId", "StyleId",
            "The Id of the Style of the Parcel.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilParcelProperties(GH_ParamAccess.item), "Properties", "Props",
            "Parcel properties (use Parcel Properties component to extract values).", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilParcelSegment(GH_ParamAccess.list), "Segments", "Seg",
            "The individual boundary segments (Lines, Arcs) of the Parcel.", GH_ParamAccess.list);

        pManager.AddCurveParameter("Boundary", "B",
            "The parcel boundary as a closed Rhino curve.", GH_ParamAccess.item);

        pManager.AddPointParameter("Centroid", "C",
            "The centroid of the parcel.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilParcel? parcelGoo = null;

        if (!DA.GetData(0, ref parcelGoo) || parcelGoo is null) return;

        var parcelId = parcelGoo.Reference.ObjectId;

        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var transactionManager = document.CreateTransactionManager();

        var parcel = transactionManager.PerformTask(() =>
            transactionManager.Unwrap().GetObject(parcelId.Unwrap(), OpenMode.ForRead) as
            Parcel);

        if (parcel == null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to read Parcel");
            return;
        }

        DA.SetData(0, new GH_AutocadObjectId(parcelId));

        DA.SetData(1, new GH_AutocadObjectId(new AutocadObjectIdWrapper(parcel.StyleId)));

        DA.SetData(2, new GH_CivilParcelProperties(CivilParcelProperties.CreateFromParcel(parcel)));

        var parcelWrapper = new CivilParcelWrapper(parcel);

        DA.SetDataList(3, parcelWrapper.Segments.Select(seg => new GH_CivilParcelSegment(seg)).ToList());
        DA.SetData(4, parcelWrapper.BoundaryCurve);
        DA.SetData(5, parcelWrapper.Centroid);
    }
}
