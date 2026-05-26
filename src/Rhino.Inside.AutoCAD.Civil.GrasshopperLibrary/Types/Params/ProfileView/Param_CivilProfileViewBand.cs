using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D ProfileView bands.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilProfileViewBand"/> objects which
/// contain band information from ProfileViews.
/// </remarks>
public class Param_CivilProfileViewBand : GH_Param<GH_CivilProfileViewBand>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("C3D4E5F6-A7B8-9012-CD34-EF56AB789012");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilProfileViewBand;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileViewBand"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilProfileViewBand(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileViewBand"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilProfileViewBand(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileViewBand"/> class.
    /// </summary>
    public Param_CivilProfileViewBand(GH_ParamAccess access)
        : base("Civil3d ProfileView Band", "PVBand",
            "A band from a Civil 3D ProfileView", "Params", "Civil3d", access)
    { }
}
