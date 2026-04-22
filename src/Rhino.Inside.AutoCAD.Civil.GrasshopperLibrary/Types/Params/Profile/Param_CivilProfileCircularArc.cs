using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Profile circular arc entities.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilProfileCircularArc"/> objects which
/// contain circular arc entity data from Profiles.
/// </remarks>
public class Param_CivilProfileCircularArc : GH_Param<GH_CivilProfileCircularArc>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("B5C6D7E8-F9A0-1234-B345-678901234EF0");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilProfileCircularArc;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileCircularArc"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilProfileCircularArc(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileCircularArc"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilProfileCircularArc(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileCircularArc"/> class.
    /// </summary>
    public Param_CivilProfileCircularArc(GH_ParamAccess access)
        : base("Civil3d Profile Circular Arc", "ProfileArc",
            "A circular arc entity from a Civil 3D Profile", "Params", "Civil3d", access)
    { }
}
