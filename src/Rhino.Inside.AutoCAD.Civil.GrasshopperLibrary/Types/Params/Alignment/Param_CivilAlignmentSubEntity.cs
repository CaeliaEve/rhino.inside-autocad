using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Alignment sub-entities.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilAlignmentSubEntity"/> objects which
/// contain individual alignment sub-entity data (Lines, Arcs, Spirals) from within an alignment entity.
/// </remarks>
public class Param_CivilAlignmentSubEntity : GH_Param<GH_CivilAlignmentSubEntity>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("D5E6F7A8-B9C0-1234-EF01-456789012CDE");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilAlignmentSubEntity;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentSubEntity"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilAlignmentSubEntity(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentSubEntity"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilAlignmentSubEntity(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentSubEntity"/> class.
    /// </summary>
    public Param_CivilAlignmentSubEntity(GH_ParamAccess access)
        : base("Civil3d Alignment Sub-Entity", "AlignSubEntity",
            "A sub-entity (Line, Arc, Spiral) from a Civil 3D Alignment Entity", "Params", "Civil3d", access)
    { }
}
