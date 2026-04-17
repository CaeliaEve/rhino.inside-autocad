using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Profile tangent entities.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilProfileTangent"/> objects which
/// contain tangent (straight line) entity data from Profiles.
/// </remarks>
public class Param_CivilProfileTangent : GH_Param<GH_CivilProfileTangent>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("A4B5C6D7-E8F9-0123-A234-567890123DEF");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileTangent"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilProfileTangent(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileTangent"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilProfileTangent(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileTangent"/> class.
    /// </summary>
    public Param_CivilProfileTangent(GH_ParamAccess access)
        : base("Civil3d Profile Tangent", "ProfileTangent",
            "A tangent (straight line) entity from a Civil 3D Profile", "Params", "Civil3d", access)
    { }
}
