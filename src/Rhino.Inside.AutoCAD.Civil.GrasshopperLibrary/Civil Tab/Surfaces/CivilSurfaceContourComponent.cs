using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that deconstructs a Civil 3D surface contour into its properties.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilSurfaceContourComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("E0F1A2B3-4C5D-6E7F-8A9B-0C1D2E3F4A5B");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilSurfaceContour;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilSurfaceContourComponent"/> class.
    /// </summary>
    public CivilSurfaceContourComponent()
        : base("Civil3d Surface Contour", "CVL-Contour",
            "Deconstructs a Civil 3D surface contour into its properties",
            "Civil3d", "Surfaces")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilSurfaceContour(), "Contour",
            "C", "A surface contour to deconstruct", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Type", "T",
            "Contour type (Major or Minor)", GH_ParamAccess.item);

        pManager.AddCurveParameter("Curve", "C",
            "The contour curve geometry", GH_ParamAccess.item);

        pManager.AddNumberParameter("Elevation", "E",
            "The elevation of the contour", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilSurfaceContour? contourGoo = null;

        if (!DA.GetData(0, ref contourGoo) || contourGoo?.Value is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No contour provided");
            return;
        }

        var contour = contourGoo.Value;

        // Output contour type as string
        var typeString = contour.ContourType switch
        {
            1 => "Major",
            2 => "Minor",
            _ => "Unknown"
        };
        DA.SetData(0, typeString);

        // Output curve
        if (contour.Curve != null && contour.Curve.IsValid)
        {
            DA.SetData(1, contour.Curve);
        }

        // Output elevation
        DA.SetData(2, contour.Elevation);
    }
}
