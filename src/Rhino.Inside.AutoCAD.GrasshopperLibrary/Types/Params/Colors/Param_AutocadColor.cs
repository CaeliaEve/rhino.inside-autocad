using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Grasshopper parameter for AutoCAD colors with ByLayer/ByBlock support.
/// </summary>
/// <remarks>
/// This parameter type supports AutoCAD's special color modes:
/// <list type="bullet">
/// <item><description>ByLayer (ColorIndex=256) - color inherited from layer</description></item>
/// <item><description>ByBlock (ColorIndex=0) - color inherited from containing block</description></item>
/// <item><description>ACI (ColorIndex=1-255) - AutoCAD Color Index colors</description></item>
/// <item><description>RGB - true color values</description></item>
/// </list>
/// </remarks>
public class Param_AutocadColor : GH_Param<GH_AutocadColor>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("a59824ca-dd57-4c70-a21e-3cdef5398ec2");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_AutocadColor;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_AutocadColor"/> class with the
    /// specified instance description.
    /// </summary>
    /// <param name="tag">The instance description for this parameter.</param>
    public Param_AutocadColor(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_AutocadColor"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    /// <param name="tag">The instance description for this parameter.</param>
    /// <param name="access">The parameter access type.</param>
    public Param_AutocadColor(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_AutocadColor"/> class with the
    /// specified parameter access type.
    /// </summary>
    /// <param name="access">The parameter access type.</param>
    public Param_AutocadColor(GH_ParamAccess access)
        : base("AutoCAD Color", "AC-Col",
            "An AutoCAD color (RGB, ByLayer, ByBlock, or ACI index). " +
            "Special values: 256=ByLayer, 0=ByBlock.",
            "Params", "AutoCAD", access)
    { }
}
