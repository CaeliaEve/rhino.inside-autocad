using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D alignment label set styles.
/// </summary>
/// <remarks>
/// This parameter supports direct string input for style names, which will be
/// automatically resolved to the corresponding alignment label set style ObjectId in the
/// active Civil 3D document.
/// </remarks>
public class Param_CivilAlignmentLabelSetStyle : GH_Param<GH_CivilAlignmentLabelSetStyle>, IReferenceParam
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("F0A1B2C3-D4E5-6F7A-8B9C-0D1E2F3A4B5C");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentLabelSetStyle"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilAlignmentLabelSetStyle(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentLabelSetStyle"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilAlignmentLabelSetStyle(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAlignmentLabelSetStyle"/> class.
    /// </summary>
    public Param_CivilAlignmentLabelSetStyle(GH_ParamAccess access)
        : base("Civil3d Alignment Label Set Style", "LblSetStyle",
            "An Alignment Label Set Style in Civil 3D. Can be a style name (string) or a style object.",
            "Params", "Civil3d", access)
    { }

    /// <inheritdoc />
    public bool NeedsToBeExpired(IAutocadDocumentChange change)
    {
        foreach (var styleGoo in m_data.AllData(true).OfType<GH_CivilAlignmentLabelSetStyle>())
        {
            if (styleGoo.Value != null && change.DoesEffectObject(styleGoo.Value.Id))
                return true;
        }

        return false;
    }
}
