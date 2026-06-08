using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for NamedId values.
/// </summary>
/// <remarks>
/// A NamedId combines a display name with an AutoCAD ObjectId reference.
/// Used for referencing named Civil 3D objects like Sites, Styles, and DesignCheckSets.
/// </remarks>
public class Param_NamedId : GH_Param<GH_NamedId>, IReferenceParam
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("b4e9f8d2-5c1a-4e8b-9d6f-3e8a7b4c9d5e");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_AutocadId;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_NamedId"/> class with the
    /// specified instance description.
    /// </summary>
    /// <param name="tag">The instance description.</param>
    public Param_NamedId(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_NamedId"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    /// <param name="tag">The instance description.</param>
    /// <param name="access">The parameter access type.</param>
    public Param_NamedId(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_NamedId"/> class with the
    /// specified parameter access type.
    /// </summary>
    /// <param name="access">The parameter access type.</param>
    public Param_NamedId(GH_ParamAccess access)
        : base("Named Id", "NamedId",
            "A name combined with an AutoCAD ObjectId reference", "Params", "AutoCAD", access)
    {
    }

    /// <inheritdoc />
    public bool NeedsToBeExpired(IAutocadDocumentChange change, bool includeModified = true)
    {
        foreach (var namedId in m_data.AllData(true).OfType<GH_NamedId>())
        {
            if (namedId.Value?.ObjectId is IObjectId objectId &&
                change.DoesEffectObject(objectId, includeModified))
                return true;
        }

        return false;
    }
}
