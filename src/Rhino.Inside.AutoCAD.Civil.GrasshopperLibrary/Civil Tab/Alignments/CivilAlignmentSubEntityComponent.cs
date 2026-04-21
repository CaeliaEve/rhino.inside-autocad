using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from a Civil 3D Alignment Sub-Entity.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilAlignmentSubEntityComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("B8C9D0E1-F2A3-4567-1234-78901BCDEF23");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilAlignmentSubEntityComponent"/> class.
    /// </summary>
    public CivilAlignmentSubEntityComponent()
        : base("Civil3d Alignment Sub-Entity", "CVL-AlignSubEntity",
            "Extracts individual values from a Civil 3D Alignment Sub-Entity",
            "Civil3d", "Alignments")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilAlignmentSubEntity(GH_ParamAccess.item), "Sub-Entity",
            "SE", "A sub-entity from a Civil3d Alignment Entity", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Type", "T",
            "The type of sub-entity (Line, Arc, Spiral).", GH_ParamAccess.item);

        pManager.AddNumberParameter("Start Station", "StaSt",
            "The starting station of this sub-entity along the alignment.", GH_ParamAccess.item);

        pManager.AddNumberParameter("End Station", "StaEnd",
            "The ending station of this sub-entity along the alignment.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Length", "Len",
            "The length of this sub-entity.", GH_ParamAccess.item);

        pManager.AddIntegerParameter("Index", "Idx",
            "The index of this sub-entity within its parent entity.", GH_ParamAccess.item);

        pManager.AddCurveParameter("Curve", "C",
            "The sub-entity geometry as a Rhino curve.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilAlignmentSubEntity? subEntityGoo = null;

        if (!DA.GetData(0, ref subEntityGoo) || subEntityGoo?.Value is null) return;

        var subEntity = subEntityGoo.Value;

        DA.SetData(0, subEntity.EntityType);
        DA.SetData(1, subEntity.StartStation);
        DA.SetData(2, subEntity.EndStation);
        DA.SetData(3, subEntity.Length);
        DA.SetData(4, subEntity.EntityIndex);
        DA.SetData(5, subEntity.ToRhinoCurve());
    }
}
