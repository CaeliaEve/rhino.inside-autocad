using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Profile label groups.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilProfileLabelGroupWrapper"/> which provides
/// properties from a Profile label group. The underlying wrapper inherits from
/// <see cref="AutocadDbObjectWrapper"/> providing access to the Civil 3D ProfileLabelGroup object.
/// </remarks>
public class GH_CivilProfileLabelGroup : GH_Goo<CivilProfileLabelGroupWrapper>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileLabelGroup"/> class with no value.
    /// </summary>
    public GH_CivilProfileLabelGroup()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileLabelGroup"/> class with the
    /// specified label group wrapper.
    /// </summary>
    /// <param name="labelGroup">The Civil 3D profile label group.</param>
    public GH_CivilProfileLabelGroup(CivilProfileLabelGroupWrapper labelGroup) : base(labelGroup)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileLabelGroup"/> class with the
    /// specified label group wrapper.
    /// </summary>
    /// <param name="labelGroup">The Civil 3D profile label group.</param>
    public GH_CivilProfileLabelGroup(ICivilProfileLabelGroup labelGroup) : base(labelGroup as CivilProfileLabelGroupWrapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileLabelGroup"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilProfileLabelGroup(GH_CivilProfileLabelGroup other)
        : base(other.Value?.ShallowClone() as CivilProfileLabelGroupWrapper)
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
    public override string TypeName => "Civil3d Profile Label Group";

    /// <inheritdoc />
    public override string TypeDescription => "A label group from a Civil 3D Profile";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilProfileLabelGroup(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilProfileLabelGroup goo)
        {
            this.Value = (goo.Value as IDbObject)?.ShallowClone() as CivilProfileLabelGroupWrapper;
            return this.Value != null;
        }

        if (source is CivilProfileLabelGroupWrapper wrapper)
        {
            this.Value = (CivilProfileLabelGroupWrapper)wrapper.ShallowClone();
            return true;
        }

        if (source is ICivilProfileLabelGroup labelGroup)
        {
            this.Value = (labelGroup as IDbObject)?.ShallowClone() as CivilProfileLabelGroupWrapper;
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilProfileLabelGroupWrapper)))
        {
            target = (Q)(object)this.Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilProfileLabelGroup)))
        {
            target = (Q)(object)new GH_CivilProfileLabelGroup(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Civil3d Profile Label Group";

        return this.Value.ToString() ?? "Civil3d Profile Label Group";
    }
}
