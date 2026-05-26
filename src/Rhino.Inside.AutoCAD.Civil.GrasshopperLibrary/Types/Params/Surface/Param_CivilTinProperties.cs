using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D TIN surface properties.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilTinProperties"/> objects which
/// contain general statistics from TIN Surfaces.
/// </remarks>
public class Param_CivilTinProperties : GH_Param<GH_CivilTinProperties>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("B2C3D4E5-F6A7-8901-BCDE-F12345678901");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilTinProperties;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilTinProperties"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilTinProperties(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilTinProperties"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilTinProperties(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilTinProperties"/> class.
    /// </summary>
    public Param_CivilTinProperties(GH_ParamAccess access)
        : base("Civil3d TIN Properties", "TINProps",
            "General statistics from a Civil 3D TIN Surface", "Params", "Civil3d", access)
    { }
}
