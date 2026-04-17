using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D alignment composite sub-entities.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilAlignmentComposite"/> objects which
/// contain composite (multi-segment) entities extracted from alignment horizontal geometry.
/// </remarks>
public class Param_CivilAlignmentComposite : GH_Param<GH_CivilAlignmentComposite>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567804");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilAlignmentComposite;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentComposite"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilAlignmentComposite(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentComposite"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilAlignmentComposite(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentComposite"/> class with
    /// the specified parameter access type.
    /// </summary>
    public Param_CivilAlignmentComposite(GH_ParamAccess access)
        : base("Civil3d Alignment Composite", "AlnComposite",
            "A composite sub-entity from a Civil 3D Alignment", "Params", "Civil3d", access)
    {
    }
}
