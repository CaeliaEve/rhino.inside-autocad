using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that deconstructs a Civil 3D surface boundary into its properties.
/// </summary>
[ComponentVersion(introduced: "1.1.19")]
public class CivilSurfaceBoundaryComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("D6E7F8A9-0B1C-2D3E-4F5A-6B7C8D9E0F1A");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilSurfaceBoundary;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.tertiary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilSurfaceBoundaryComponent"/> class.
    /// </summary>
    public CivilSurfaceBoundaryComponent()
        : base("Civil3d Surface Boundary", "CVL-Boundary",
            "Deconstructs a Civil 3D surface boundary into its properties",
            "Civil3d", "Surfaces")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilSurfaceBoundary(GH_ParamAccess.item), "Boundary",
            "B", "A surface boundary to deconstruct", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Type", "T",
            "Boundary type (Outer, DataClip, Hide, or Show)", GH_ParamAccess.item);

        pManager.AddCurveParameter("Polyline", "P",
            "The boundary polyline geometry", GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The boundary name", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilSurfaceBoundary? boundaryGoo = null;

        if (!DA.GetData(0, ref boundaryGoo) || boundaryGoo?.Value is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No boundary provided");
            return;
        }

        var boundary = boundaryGoo.Value;

        DA.SetData(0, boundary.BoundaryType.ToString());

        DA.SetData(1, boundary.Curve);

        DA.SetData(2, boundary.Name);
    }
}
