using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Profile properties.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilProfileProperties"/> objects which
/// contain properties from Profiles.
/// </remarks>
public class Param_CivilProfileProperties : GH_Param<GH_CivilProfileProperties>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("E2F3A4B5-C6D7-8901-EF12-345678901BCD");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilProfileProperties;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileProperties"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilProfileProperties(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileProperties"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilProfileProperties(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileProperties"/> class.
    /// </summary>
    public Param_CivilProfileProperties(GH_ParamAccess access)
        : base("Civil3d Profile Properties", "ProfileProps",
            "Properties from a Civil 3D Profile", "Params", "Civil3d", access)
    { }
}
