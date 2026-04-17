using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Sites.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilSite"/> objects which
/// contain site information and collections.
/// </remarks>
public class Param_CivilSite : GH_Param<GH_CivilSite>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("E5F6A7B8-C9D0-1234-EF01-567890123456");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSite"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilSite(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSite"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilSite(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSite"/> class.
    /// </summary>
    public Param_CivilSite(GH_ParamAccess access)
        : base("Civil3d Site", "Site",
            "A Civil 3D Site container", "Params", "Civil3d", access)
    { }
}
