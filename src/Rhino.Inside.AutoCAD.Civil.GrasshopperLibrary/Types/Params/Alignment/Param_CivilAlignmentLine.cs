using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D alignment line sub-entities.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilAlignmentLine"/> objects which
/// contain line segments extracted from alignment horizontal geometry.
/// </remarks>
public class Param_CivilAlignmentLine : GH_Param<GH_CivilAlignmentLine>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567801");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilAlignmentLine;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentLine"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilAlignmentLine(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentLine"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilAlignmentLine(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentLine"/> class with
    /// the specified parameter access type.
    /// </summary>
    public Param_CivilAlignmentLine(GH_ParamAccess access)
        : base("Civil3d Alignment Line", "AlnLine",
            "A line sub-entity from a Civil 3D Alignment", "Params", "Civil3d", access)
    {
    }
}
