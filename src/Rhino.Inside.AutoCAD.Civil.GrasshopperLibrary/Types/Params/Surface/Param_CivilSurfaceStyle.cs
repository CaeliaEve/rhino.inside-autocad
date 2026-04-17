using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D surface styles.
/// </summary>
/// <remarks>
/// This parameter supports direct string input for style names, which will be
/// automatically resolved to the corresponding surface style ObjectId in the
/// active Civil 3D document.
/// </remarks>
public class Param_CivilSurfaceStyle : GH_Param<GH_CivilSurfaceStyle>, IReferenceParam
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("D8E3F4A1-7B2C-4D5E-9A6F-1C8B3E4D5F6A");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilSurfaceStyle;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSurfaceStyle"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilSurfaceStyle(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSurfaceStyle"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilSurfaceStyle(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSurfaceStyle"/> class.
    /// </summary>
    public Param_CivilSurfaceStyle(GH_ParamAccess access)
        : base("Civil3d Surface Style", "Style",
            "A Surface Style in Civil 3D", "Params", "Civil3d", access)
    { }

    /// <inheritdoc />
    public bool NeedsToBeExpired(IAutocadDocumentChange change)
    {
        foreach (var styleGoo in m_data.AllData(true).OfType<GH_CivilSurfaceStyle>())
        {
            if (styleGoo.Value != null && change.DoesEffectObject(styleGoo.Value.Id))
                return true;
        }

        return false;
    }
}
