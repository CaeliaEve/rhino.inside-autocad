using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using RhinoCurve = Rhino.Geometry.PolylineCurve;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that calculates cut/fill volumes within a custom polygon boundary
/// on a Civil 3D TIN Volume Surface.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilBoundedVolumeComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("E9F1A2B3-4C5D-6E7F-8A9B-0C1D2E3F4A5B");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilBoundedVolumeComponent;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.tertiary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilBoundedVolumeComponent"/> class.
    /// </summary>
    public CivilBoundedVolumeComponent()
        : base("Civil3d Bounded Volume", "CVL-BoundedVol",
            "Calculates cut/fill volumes within a custom polygon boundary on a Volume Surface",
            "Civil3d", "Surfaces")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilTinVolumeSurface(), "Volume Surface", "VolSrf",
            "The volume surface to analyze.", GH_ParamAccess.item);

        pManager.AddCurveParameter("Boundary", "B",
            "Closed polygon defining the region for volume calculation.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Datum", "D",
            "Elevation datum for volume calculation (default 0.0).", GH_ParamAccess.item, 0.0);
        pManager[2].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddNumberParameter("Cut Volume", "Cut",
            "Cut volume within the bounded region.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Fill Volume", "Fill",
            "Fill volume within the bounded region.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Net Volume", "Net",
            "Net volume within the bounded region (cut - fill).", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilTinVolumeSurface? volumeSurfaceGoo = null;
        RhinoCurve? boundaryCurve = null;
        var datum = 0.0;

        if (!DA.GetData(0, ref volumeSurfaceGoo) || volumeSurfaceGoo is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No volume surface provided");
            return;
        }

        if (!DA.GetData(1, ref boundaryCurve) || boundaryCurve is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No boundary curve provided");
            return;
        }

        DA.GetData(2, ref datum);

        // Validate that the curve is closed
        if (!boundaryCurve.IsClosed)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Boundary curve must be closed");
            return;
        }

        var surfaceId = volumeSurfaceGoo.Reference.ObjectId;

        var document = this.GetDocumentForObjectId(surfaceId);
        if (document is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No document available");
            return;
        }

        // Convert Rhino curve to point collection
        var points = ConvertCurveToPoints(boundaryCurve);

        if (points.Count < 3)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Boundary must have at least 3 points");
            return;
        }

        var transactionManager = document.CreateTransactionManager();

        var result = transactionManager.PerformTask(() =>
        {
            try
            {
                var volumeSurface = transactionManager.Unwrap()
                    .GetObject(surfaceId.Unwrap(), OpenMode.ForRead) as TinVolumeSurface;

                if (volumeSurface == null)
                {
                    return (false, 0.0, 0.0, 0.0, "Failed to read Volume Surface");
                }

                // Use GetBoundedVolumes to calculate volumes within the boundary
                var boundedVolumes = volumeSurface.GetBoundedVolumes(points, datum);

                var cutVolume = boundedVolumes.Cut;
                var fillVolume = boundedVolumes.Fill;
                var netVolume = cutVolume - fillVolume;

                return (true, cutVolume, fillVolume, netVolume, string.Empty);
            }
            catch (Autodesk.Civil.CivilException ex)
            {
                return (false, 0.0, 0.0, 0.0, $"Civil 3D error: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                return (false, 0.0, 0.0, 0.0, $"Error calculating bounded volumes: {ex.Message}");
            }
        });

        var (success, cut, fill, net, error) = result;

        if (!success)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error);
            return;
        }

        DA.SetData(0, cut);
        DA.SetData(1, fill);
        DA.SetData(2, net);
    }

    /// <summary>
    /// Converts a Rhino curve to an AutoCAD Point3dCollection.
    /// </summary>
    private static Point3dCollection ConvertCurveToPoints(PolylineCurve curve)
    {
        var points = new Point3dCollection();

        // Try to get a polyline from the curve
        if (curve.TryGetPolyline(out var polyline))
        {
            foreach (var pt in polyline)
            {
                points.Add(pt.ToAutocadPoint3d());
            }
        }
        else
        {
            // Divide curve into segments
            var curveLength = curve.GetLength();
            var segmentCount = Math.Max(10, (int)(curveLength / 1.0)); // Approximate 1 unit spacing

            var divisionParams = curve.DivideByCount(segmentCount, true);

            if (divisionParams != null)
            {
                foreach (var t in divisionParams)
                {
                    var pt = curve.PointAt(t);
                    points.Add(pt.ToAutocadPoint3d());
                }
            }
        }

        return points;
    }
}
