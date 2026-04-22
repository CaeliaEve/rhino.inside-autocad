using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts information from a Civil 3D Corridor Surface.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilCorridorSurfaceComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("A3B4C5D6-E7F8-9012-3456-789012345EF0");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilCorridorSurfaceComponent;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilCorridorSurfaceComponent"/> class.
    /// </summary>
    public CivilCorridorSurfaceComponent()
        : base("Civil3d Corridor Surface", "CVL-CorrSrf",
            "Extracts information from a Civil 3D Corridor Surface",
            "Civil3d", "Corridors")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilCorridorSurface(GH_ParamAccess.item), "Surface",
            "Srf", "A Corridor surface", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "N",
            "The name of the corridor surface.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Surface Id", "SrfId",
            "The Id of the generated TIN surface.", GH_ParamAccess.item);

        pManager.AddMeshParameter("Mesh", "M",
            "The surface as a Rhino mesh.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilCorridorSurface? surfaceGoo = null;

        if (!DA.GetData(0, ref surfaceGoo) || surfaceGoo?.Value is null) return;

        var surface = surfaceGoo.Value;

        DA.SetData(0, surface.Name);
        DA.SetData(1, new GH_AutocadObjectId(surface.SurfaceId));
        DA.SetData(2, surface.Mesh);
    }
}
