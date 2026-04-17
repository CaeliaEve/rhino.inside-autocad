using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from a Civil 3D Alignment Entity.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilAlignmentEntityComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("A7B8C9D0-E1F2-3456-0123-67890ABCDEF0");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilAlignmentEntityComponent"/> class.
    /// </summary>
    public CivilAlignmentEntityComponent()
        : base("Civil3d Alignment Entity", "CVL-AlignEntity",
            "Extracts individual values from a Civil 3D Alignment Entity",
            "Civil3d", "Alignments")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilAlignmentEntity(GH_ParamAccess.item), "Entity",
            "E", "An alignment entity from a Civil3d Alignment", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Type", "T",
            "The type of entity (Line, Arc, Spiral, etc.).", GH_ParamAccess.item);

        pManager.AddNumberParameter("Start Station", "StaSt",
            "The starting station of this entity along the alignment.", GH_ParamAccess.item);

        pManager.AddNumberParameter("End Station", "StaEnd",
            "The ending station of this entity along the alignment.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Length", "Len",
            "The length of this entity.", GH_ParamAccess.item);

        pManager.AddIntegerParameter("Index", "Idx",
            "The index of this entity in the alignment.", GH_ParamAccess.item);

        pManager.AddCurveParameter("Curve", "C",
            "The entity geometry as a Rhino curve.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilAlignmentEntity? entityGoo = null;

        if (!DA.GetData(0, ref entityGoo) || entityGoo?.Value is null) return;

        var entity = entityGoo.Value;

        DA.SetData(0, entity.EntityType);
        DA.SetData(1, entity.StartStation);
        DA.SetData(2, entity.EndStation);
        DA.SetData(3, entity.Length);
        DA.SetData(4, entity.EntityIndex);
        DA.SetData(5, entity.Curve);
    }
}
