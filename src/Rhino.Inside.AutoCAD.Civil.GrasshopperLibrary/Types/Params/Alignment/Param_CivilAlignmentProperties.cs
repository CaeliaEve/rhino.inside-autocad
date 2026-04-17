using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Alignment properties.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilAlignmentProperties"/> objects which
/// contain properties from Alignments.
/// </remarks>
public class Param_CivilAlignmentProperties : GH_Param<GH_CivilAlignmentProperties>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("B3C4D5E6-F7A8-9012-CDEF-234567890ABC");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentProperties"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilAlignmentProperties(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentProperties"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilAlignmentProperties(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentProperties"/> class.
    /// </summary>
    public Param_CivilAlignmentProperties(GH_ParamAccess access)
        : base("Civil3d Alignment Properties", "AlignProps",
            "Properties from a Civil 3D Alignment", "Params", "Civil3d", access)
    { }
}
