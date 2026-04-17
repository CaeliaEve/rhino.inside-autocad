using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Alignment entities.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilAlignmentEntity"/> objects which
/// contain individual alignment entity data (Lines, Arcs, Spirals).
/// </remarks>
public class Param_CivilAlignmentEntity : GH_Param<GH_CivilAlignmentEntity>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("C4D5E6F7-A8B9-0123-DEF0-345678901BCD");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentEntity"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilAlignmentEntity(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentEntity"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilAlignmentEntity(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentEntity"/> class.
    /// </summary>
    public Param_CivilAlignmentEntity(GH_ParamAccess access)
        : base("Civil3d Alignment Entity", "AlignEntity",
            "An entity (Line, Arc, Spiral) from a Civil 3D Alignment", "Params", "Civil3d", access)
    { }
}
