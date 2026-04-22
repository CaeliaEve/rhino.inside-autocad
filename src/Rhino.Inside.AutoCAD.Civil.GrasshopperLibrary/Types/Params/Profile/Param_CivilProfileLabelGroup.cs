using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Profile label groups.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilProfileLabelGroup"/> objects which
/// contain label group data from Profiles.
/// </remarks>
public class Param_CivilProfileLabelGroup : GH_Param<GH_CivilProfileLabelGroup>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("B2C3D4E5-F6A7-8901-BCDE-F23456789012");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilProfileLabelGroup;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileLabelGroup"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilProfileLabelGroup(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileLabelGroup"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilProfileLabelGroup(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileLabelGroup"/> class.
    /// </summary>
    public Param_CivilProfileLabelGroup(GH_ParamAccess access)
        : base("Civil3d Profile Label Group", "ProfLblGrp",
            "A label group from a Civil 3D Profile", "Params", "Civil3d", access)
    { }
}
