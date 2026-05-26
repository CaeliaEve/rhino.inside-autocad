using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Alignment label groups.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilAlignmentLabelGroup"/> objects which
/// contain label group data from Alignments.
/// </remarks>
public class Param_CivilAlignmentLabelGroup : GH_Param<GH_CivilAlignmentLabelGroup>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567805");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilAlignmentLabelGroup;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentLabelGroup"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilAlignmentLabelGroup(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentLabelGroup"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilAlignmentLabelGroup(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentLabelGroup"/> class.
    /// </summary>
    public Param_CivilAlignmentLabelGroup(GH_ParamAccess access)
        : base("Civil3d Alignment Label Group", "AlignLblGrp",
            "A label group from a Civil 3D Alignment", "Params", "Civil3d", access)
    { }
}
