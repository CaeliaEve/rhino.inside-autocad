using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Sites.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilSite"/> objects which
/// contain site information and collections.
/// </remarks>
public class Param_CivilSite : GH_Param<GH_CivilSite>, IReferenceParam
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("E5F6A7B8-C9D0-1234-EF01-567890123456");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilSite;

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

    /// <inheritdoc />
    public bool NeedsToBeExpired(IAutocadDocumentChange change, bool includeModified = true)
    {
        foreach (var site in m_data.AllData(true).OfType<GH_CivilSite>())
        {
            if (site.Value?.Id != null && change.DoesEffectObject(site.Value.Id, includeModified))
                return true;
        }
        return false;
    }
}
