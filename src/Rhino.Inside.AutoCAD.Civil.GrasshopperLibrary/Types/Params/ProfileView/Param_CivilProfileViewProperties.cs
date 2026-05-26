using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D ProfileView properties.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilProfileViewProperties"/> objects which
/// contain properties from ProfileViews.
/// </remarks>
public class Param_CivilProfileViewProperties : GH_Param<GH_CivilProfileViewProperties>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("B2C3D4E5-F6A7-8901-BC23-DE45FA678901");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilProfileViewProperties;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileViewProperties"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilProfileViewProperties(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileViewProperties"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilProfileViewProperties(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileViewProperties"/> class.
    /// </summary>
    public Param_CivilProfileViewProperties(GH_ParamAccess access)
        : base("Civil3d ProfileView Properties", "PVProps",
            "Properties from a Civil 3D ProfileView", "Params", "Civil3d", access)
    { }
}
