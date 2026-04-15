using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using TinSurface = Autodesk.Civil.DatabaseServices.TinSurface;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts contour curves from a Civil 3D TIN Surface.
/// </summary>
/// <remarks>
/// This component uses the TinSurface.ExtractContours methods to generate actual
/// contour polyline geometry from a TIN surface at specified intervals.
/// </remarks>
[ComponentVersion(introduced: "1.2.19")]
public class ExtractContoursComponent : RhinoInsideAutocad_ComponentBase
{

    private string _errorMessage = string.Empty;

    /// <inheritdoc />
    public override Guid ComponentGuid => new("A3B7C9D1-5E2F-4A8B-9C6D-7E4F3A2B1C8D");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractContoursComponent"/> class.
    /// </summary>
    public ExtractContoursComponent()
        : base("Extract Contours", "CVL-ExtCtr",
            "Extracts contour curves from a Civil 3D TIN surface at specified intervals",
            "Civil3d", "Surfaces")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilTinSurface(), "Surface",
            "Srf", "A Civil 3D TIN Surface", GH_ParamAccess.item);

        pManager.AddNumberParameter("Interval", "Int",
            "The elevation interval between contour lines. If not provided, uses the surface's default contour settings.",
            GH_ParamAccess.item);
        pManager[1].Optional = true;

        pManager.AddIntegerParameter("ContourType", "Type",
            "Type of contours to extract: 0 = All (at interval), 1 = Major only, 2 = Minor only. Default is 0.",
            GH_ParamAccess.item, 0);
        pManager[2].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddCurveParameter("Contours", "Crv",
            "The extracted contour curves as Rhino curves.", GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilTinSurface? gooSurface = null;
        var interval = 1.0;
        var contourType = 0;

        if (!DA.GetData(0, ref gooSurface) || gooSurface is null) return;

        DA.GetData(1, ref interval);
        DA.GetData(2, ref contourType);

        // Validate contour type
        if (contourType < 0 || contourType > 2)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                "ContourType must be 0 (All), 1 (Major), or 2 (Minor). Using 0 (All).");
            contourType = 0;
        }

        // Get the Reference ObjectId (the actual database reference, not the clone's ObjectId)
        var surfaceId = gooSurface.Reference.ObjectId;
        if (!surfaceId.IsValid)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                "Surface does not have a valid database reference.");
            return;
        }

        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        _errorMessage = string.Empty;

        var rhinoCurvesOut = document.Transaction((transactionManager) =>
        {
            var rhinoCurves = new List<Rhino.Geometry.Curve>();

            try
            {
                var transaction = transactionManager.Unwrap();

                var surface =
                    transaction.GetObject(surfaceId.Unwrap(), OpenMode.ForRead) as TinSurface;
                if (surface is null)
                {
                    _errorMessage = "Failed to open TIN Surface for reading.";
                    return rhinoCurves;
                }

                ObjectIdCollection? contourIds = null;

                switch (contourType)
                {
                    case 1: // Major contours only
                        contourIds =
                            surface.ExtractMajorContours(SurfaceExtractionSettingsType
                                .Model);
                        break;

                    case 2: // Minor contours only
                        contourIds =
                            surface.ExtractMinorContours(SurfaceExtractionSettingsType
                                .Model);
                        break;

                    default: // All contours at interval
                        contourIds = surface.ExtractContours(interval);
                        break;
                }

                foreach (ObjectId contourId in contourIds)
                {
                    if (contourId.IsNull || contourId.IsErased || !contourId.IsValid)
                        continue;

                    var dbObject = transaction.GetObject(contourId, OpenMode.ForRead);

                    if (dbObject is Curve curve)
                    {
                        var rhinoCurve = curve.ToRhinoCurve();
                        if (rhinoCurve != null)
                            rhinoCurves.Add(rhinoCurve);
                    }
                }
            }
            catch (System.Exception ex)
            {
                _errorMessage = $"Failed to extract contours: {ex.Message}";
            }

            return rhinoCurves;

        });

        if (rhinoCurvesOut.Any() == false || string.IsNullOrEmpty(_errorMessage) == false)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, _errorMessage);
        }

        DA.SetDataList(0, rhinoCurvesOut);
    }
}
