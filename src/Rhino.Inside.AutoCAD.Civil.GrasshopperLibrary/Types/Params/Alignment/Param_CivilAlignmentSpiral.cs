using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D alignment spiral sub-entities.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilAlignmentSpiral"/> objects which
/// contain spiral (transition curve) segments extracted from alignment horizontal geometry.
/// </remarks>
public class Param_CivilAlignmentSpiral : GH_Param<GH_CivilAlignmentSpiral>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567803");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilAlignmentSpiral;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentSpiral"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilAlignmentSpiral(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentSpiral"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilAlignmentSpiral(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentSpiral"/> class with
    /// the specified parameter access type.
    /// </summary>
    public Param_CivilAlignmentSpiral(GH_ParamAccess access)
        : base("Civil3d Alignment Spiral", "AlnSpiral",
            "A spiral sub-entity from a Civil 3D Alignment", "Params", "Civil3d", access)
    {
    }
}
