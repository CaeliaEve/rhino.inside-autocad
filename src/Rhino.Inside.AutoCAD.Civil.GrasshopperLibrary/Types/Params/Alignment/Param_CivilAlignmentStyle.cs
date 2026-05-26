using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D alignment styles.
/// </summary>
/// <remarks>
/// This parameter supports direct string input for style names, which will be
/// automatically resolved to the corresponding alignment style ObjectId in the
/// active Civil 3D document.
/// </remarks>
public class Param_CivilAlignmentStyle : GH_Param<GH_CivilAlignmentStyle>, IReferenceParam
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("E9F0A1B2-C3D4-5E6F-7A8B-9C0D1E2F3A4B");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentStyle"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilAlignmentStyle(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentStyle"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilAlignmentStyle(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentStyle"/> class.
    /// </summary>
    public Param_CivilAlignmentStyle(GH_ParamAccess access)
        : base("Civil3d Alignment Style", "AlnStyle",
            "An Alignment Style in Civil 3D. Can be a style name (string) or a style object.",
            "Params", "Civil3d", access)
    { }

    /// <inheritdoc />
    public bool NeedsToBeExpired(IAutocadDocumentChange change)
    {
        foreach (var styleGoo in m_data.AllData(true).OfType<GH_CivilAlignmentStyle>())
        {
            if (styleGoo.Value != null && change.DoesEffectObject(styleGoo.Value.Id))
                return true;
        }

        return false;
    }
}
