using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Alignment properties.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilAlignmentPropertiesWrapper"/> containing
/// properties from an Alignment.
/// </remarks>
public class GH_CivilAlignmentProperties : GH_Goo<CivilAlignmentPropertiesWrapper>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentProperties"/> class with no value.
    /// </summary>
    public GH_CivilAlignmentProperties()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentProperties"/> class with the
    /// specified alignment properties wrapper.
    /// </summary>
    /// <param name="alignmentProperties">The Civil 3D alignment properties wrapper.</param>
    public GH_CivilAlignmentProperties(CivilAlignmentPropertiesWrapper alignmentProperties) : base(alignmentProperties)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentProperties"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilAlignmentProperties(GH_CivilAlignmentProperties other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilAlignmentProperties"/> via the interface.
    /// </summary>
    public GH_CivilAlignmentProperties(ICivilAlignmentProperties alignmentProperties)
        : base((alignmentProperties as CivilAlignmentPropertiesWrapper)!)
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
                return "No alignment properties data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Alignment Properties";

    /// <inheritdoc />
    public override string TypeDescription => "Properties from a Civil 3D Alignment";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilAlignmentProperties(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilAlignmentProperties goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilAlignmentPropertiesWrapper wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilAlignmentProperties props)
        {
            Value = (props as CivilAlignmentPropertiesWrapper)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilAlignmentPropertiesWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilAlignmentProperties)))
        {
            target = (Q)(object)new GH_CivilAlignmentProperties(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Alignment Properties";

        return Value.ToString();
    }
}
