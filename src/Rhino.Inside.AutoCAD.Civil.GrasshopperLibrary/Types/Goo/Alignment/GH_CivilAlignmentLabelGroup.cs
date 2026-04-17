using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Alignment label groups.
/// </summary>
/// <remarks>
/// This Goo wraps an <see cref="CivilAlignmentLabelGroupWrapper"/> which provides
/// properties from an Alignment label group. The underlying wrapper inherits from
/// <see cref="AutocadDbObjectWrapper"/> providing access to the Civil 3D LabelGroup object.
/// </remarks>
public class GH_CivilAlignmentLabelGroup : GH_Goo<CivilAlignmentLabelGroupWrapper>
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
    /// <param name="labelGroup">The Civil 3D alignment label group.</param>
    public GH_CivilAlignmentLabelGroup(CivilAlignmentLabelGroupWrapper labelGroup) : base(labelGroup)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentLabelGroup"/> class with the
    /// specified label group wrapper.
    /// </summary>
    /// <param name="labelGroup">The Civil 3D alignment label group.</param>
    public GH_CivilAlignmentLabelGroup(ICivilAlignmentLabelGroup labelGroup) : base(labelGroup as CivilAlignmentLabelGroupWrapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentLabelGroup"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilAlignmentLabelGroup(GH_CivilAlignmentLabelGroup other)
        : base(other.Value?.ShallowClone() as CivilAlignmentLabelGroupWrapper)
    {
    }

    /// <inheritdoc />
    public override bool IsValid => this.Value != null;

    /// <inheritdoc />
    public override string IsValidWhyNot
    {
        get
        {
            if (this.Value == null)
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
            this.Value = (goo.Value as IDbObject)?.ShallowClone() as CivilAlignmentLabelGroupWrapper;
            return this.Value != null;
        }

        if (source is CivilAlignmentLabelGroupWrapper wrapper)
        {
            this.Value = (CivilAlignmentLabelGroupWrapper)wrapper.ShallowClone();
            return true;
        }

        if (source is ICivilAlignmentLabelGroup labelGroup)
        {
            this.Value = (labelGroup as IDbObject)?.ShallowClone() as CivilAlignmentLabelGroupWrapper;
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilAlignmentLabelGroupWrapper)))
        {
            target = (Q)(object)this.Value!;
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
        if (this.Value == null)
            return "Null Civil3d Alignment Label Group";

        return this.Value.ToString() ?? "Civil3d Alignment Label Group";
    }
}
