using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D TIN surface properties.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilTinPropertiesWrapper"/> containing
/// general statistics from a TIN Surface.
/// </remarks>
public class GH_CivilTinProperties : GH_Goo<CivilTinPropertiesWrapper>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilTinProperties"/> class with no value.
    /// </summary>
    public GH_CivilTinProperties()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilTinProperties"/> class with the
    /// specified TIN properties wrapper.
    /// </summary>
    /// <param name="tinProperties">The Civil 3D TIN properties wrapper.</param>
    public GH_CivilTinProperties(CivilTinPropertiesWrapper tinProperties) : base(tinProperties)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilTinProperties"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilTinProperties(GH_CivilTinProperties other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilTinProperties"/> via the interface.
    /// </summary>
    public GH_CivilTinProperties(ICivilTinProperties tinProperties)
        : base((tinProperties as CivilTinPropertiesWrapper)!)
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
                return "No TIN properties data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d TIN Properties";

    /// <inheritdoc />
    public override string TypeDescription => "General statistics from a Civil 3D TIN Surface";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilTinProperties(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilTinProperties goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilTinPropertiesWrapper wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilTinProperties props)
        {
            Value = (props as CivilTinPropertiesWrapper)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilTinPropertiesWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilTinProperties)))
        {
            target = (Q)(object)new GH_CivilTinProperties(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d TIN Properties";

        return Value.ToString();
    }
}
