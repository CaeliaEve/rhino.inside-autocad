using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from Civil 3D TIN Surface Properties.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class TINSurfacePropertiesComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("D4E5F6A7-B8C9-0123-DEF0-345678901BCD");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.TINSurfacePropertiesComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="TINSurfacePropertiesComponent"/> class.
    /// </summary>
    public TINSurfacePropertiesComponent()
        : base("Civil3d TIN Properties", "CVL-TINProps",
            "Extracts individual values from Civil 3D TIN Surface Properties",
            "Civil3d", "Surfaces")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilTinProperties(GH_ParamAccess.item), "TIN Properties",
            "TP", "TIN properties from a Civil3d Surface", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "N",
            "The name of the surface.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Minimum Elevation", "MinE",
            "The minimum elevation of the surface.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Maximum Elevation", "MaxE",
            "The maximum elevation of the surface.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Minimum X", "MinX",
            "The minimum X coordinate of the surface extent.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Maximum X", "MaxX",
            "The maximum X coordinate of the surface extent.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Minimum Y", "MinY",
            "The minimum Y coordinate of the surface extent.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Maximum Y", "MaxY",
            "The maximum Y coordinate of the surface extent.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_NamedId(GH_ParamAccess.item), "Style",
            "Style", "The style applied to this TIN surface as a NamedId.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilTinProperties? tinPropsGoo = null;

        if (!DA.GetData(0, ref tinPropsGoo) || tinPropsGoo?.Value is null) return;

        var props = tinPropsGoo.Value;

        DA.SetData(0, props.Name);
        DA.SetData(1, props.MinimumPoint.Elevation);
        DA.SetData(2, props.MaximumPoint.Elevation);
        DA.SetData(3, props.MinimumPoint.X);
        DA.SetData(4, props.MaximumPoint.X);
        DA.SetData(5, props.MinimumPoint.Y);
        DA.SetData(6, props.MaximumPoint.Y);
        DA.SetData(7, new GH_NamedId(props.Style));
    }
}
