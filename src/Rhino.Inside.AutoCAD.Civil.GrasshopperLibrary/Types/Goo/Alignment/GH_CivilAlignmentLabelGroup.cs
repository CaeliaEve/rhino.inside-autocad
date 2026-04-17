using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Alignment label groups.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilAlignmentLabelGroupWrapperBase"/> containing
/// properties from an Alignment label group. Label groups are metadata only
/// and do not have geometry to preview.
/// </remarks>
public class GH_CivilAlignmentLabelGroup : GH_Goo<CivilAlignmentLabelGroupWrapperBase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentLabelGroup"/> class with no value.
    /// </summary>
    public GH_CivilAlignmentLabelGroup()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentLabelGroup"/> class with the
    /// specified label group wrapper.
    /// </summary>
    /// <param name="labelGroup">The Civil 3D alignment label group wrapper.</param>
    public GH_CivilAlignmentLabelGroup(CivilAlignmentLabelGroupWrapperBase labelGroup) : base(labelGroup)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentLabelGroup"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilAlignmentLabelGroup(GH_CivilAlignmentLabelGroup other) : base(other.Value?.DuplicateBase())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilAlignmentLabelGroup"/> via the interface.
    /// </summary>
    public GH_CivilAlignmentLabelGroup(ICivilAlignmentLabelGroup labelGroup)
        : base((labelGroup as CivilAlignmentLabelGroupWrapperBase)!)
    {
    }

    /// <inheritdoc />
    public override bool IsValid => Value != null;

    /// <inheritdoc />
    public override string IsValidWhyNot
    {
        get
        {
            if (Value == null)
                return "No label group data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Alignment Label Group";

    /// <inheritdoc />
    public override string TypeDescription => "A label group from a Civil 3D Alignment";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilAlignmentLabelGroup(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilAlignmentLabelGroup goo)
        {
            Value = goo.Value?.DuplicateBase();
            return true;
        }

        if (source is CivilAlignmentLabelGroupWrapperBase wrapper)
        {
            Value = wrapper.DuplicateBase();
            return true;
        }

        if (source is ICivilAlignmentLabelGroup labelGroup)
        {
            Value = (labelGroup as CivilAlignmentLabelGroupWrapperBase)?.DuplicateBase();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilAlignmentLabelGroupWrapperBase)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilAlignmentLabelGroup)))
        {
            target = (Q)(object)new GH_CivilAlignmentLabelGroup(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Alignment Label Group";

        return Value.ToString();
    }
}
