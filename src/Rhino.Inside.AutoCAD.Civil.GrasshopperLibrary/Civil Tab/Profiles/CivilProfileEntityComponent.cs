using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from a Civil 3D Profile Entity.
/// </summary>
[ComponentVersion(introduced: "1.0.19")]
public class CivilProfileEntityComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("F9A0B1C2-D3E4-5678-F789-012345678CDE");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilProfileEntityComponent"/> class.
    /// </summary>
    public CivilProfileEntityComponent()
        : base("Civil3d Profile Entity", "CVL-ProfileEntity",
            "Extracts individual values from a Civil 3D Profile Entity",
            "Civil3d", "Profiles")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilProfileEntity(GH_ParamAccess.item), "Entity",
            "E", "A profile entity from a Civil3d Profile", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Type", "T",
            "The type of entity (Tangent, CircularArc, Parabola, etc.).", GH_ParamAccess.item);

        pManager.AddNumberParameter("Start Station", "StaSt",
            "The starting station of this entity along the profile.", GH_ParamAccess.item);

        pManager.AddNumberParameter("End Station", "StaEnd",
            "The ending station of this entity along the profile.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Start Elevation", "StartElev",
            "The elevation at the start of this entity.", GH_ParamAccess.item);

        pManager.AddNumberParameter("End Elevation", "EndElev",
            "The elevation at the end of this entity.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Length", "Len",
            "The length of this entity.", GH_ParamAccess.item);

        pManager.AddIntegerParameter("Index", "Idx",
            "The index of this entity in the profile.", GH_ParamAccess.item);

        pManager.AddCurveParameter("Curve", "C",
            "The entity geometry as a Rhino curve (X=Station, Y=Elevation).", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilProfileEntity? entityGoo = null;

        if (!DA.GetData(0, ref entityGoo) || entityGoo?.Value is null) return;

        var entity = entityGoo.Value;

        DA.SetData(0, entity.EntityType);
        DA.SetData(1, entity.StartStation);
        DA.SetData(2, entity.EndStation);
        DA.SetData(3, entity.StartElevation);
        DA.SetData(4, entity.EndElevation);
        DA.SetData(5, entity.Length);
        DA.SetData(6, entity.EntityIndex);
        DA.SetData(7, entity.Curve);
    }
}
