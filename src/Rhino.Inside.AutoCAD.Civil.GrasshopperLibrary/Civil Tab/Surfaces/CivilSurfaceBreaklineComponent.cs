using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that deconstructs a Civil 3D surface breakline into its properties.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilSurfaceBreaklineComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("F1A2B3C4-5D6E-7F8A-9B0C-1D2E3F4A5B6C");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Civil.GrasshopperLibrary.Properties.Resources.CivilSurfaceBreakline;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilSurfaceBreaklineComponent"/> class.
    /// </summary>
    public CivilSurfaceBreaklineComponent()
        : base("Civil3d Surface Breakline", "CVL-Breakline",
            "Deconstructs a Civil 3D surface breakline into its properties",
            "Civil3d", "Surfaces")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilSurfaceBreakline(GH_ParamAccess.item), "Breakline",
            "BL", "A surface breakline to deconstruct", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddIntegerParameter("Type", "T",
            "Breakline type (0=Standard, 1=Wall, 2=Proximity, 3=NonDestructive)", GH_ParamAccess.item);

        pManager.AddCurveParameter("Curve", "C",
            "The breakline curve geometry", GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The name of the breakline", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilSurfaceBreakline? breaklineGoo = null;

        if (!DA.GetData(0, ref breaklineGoo) || breaklineGoo?.Value is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No breakline provided");
            return;
        }

        var breakline = breaklineGoo.Value;

        // Output breakline type as integer
        DA.SetData(0, breakline.BreaklineType);

        // Output curve
        if (breakline.Curve != null && breakline.Curve.IsValid)
        {
            DA.SetData(1, breakline.Curve);
        }

        // Output name
        DA.SetData(2, breakline.Name);
    }
}
