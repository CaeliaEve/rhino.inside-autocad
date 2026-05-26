using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Profile properties.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilProfileProperties"/> containing
/// properties from a Profile.
/// </remarks>
public class GH_CivilProfileProperties : GH_Goo<CivilProfileProperties>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileProperties"/> class with no value.
    /// </summary>
    public GH_CivilProfileProperties()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileProperties"/> class with the
    /// specified profile properties wrapper.
    /// </summary>
    /// <param name="profileProperties">The Civil 3D profile properties wrapper.</param>
    public GH_CivilProfileProperties(CivilProfileProperties profileProperties) : base(profileProperties)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileProperties"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilProfileProperties(GH_CivilProfileProperties other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilProfileProperties"/> via the interface.
    /// </summary>
    public GH_CivilProfileProperties(ICivilProfileProperties profileProperties)
        : base((profileProperties as CivilProfileProperties)!)
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
                return "No profile properties data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Profile Properties";

    /// <inheritdoc />
    public override string TypeDescription => "Properties from a Civil 3D Profile";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilProfileProperties(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilProfileProperties goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilProfileProperties wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilProfileProperties props)
        {
            Value = (props as CivilProfileProperties)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilProfileProperties)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilProfileProperties)))
        {
            target = (Q)(object)new GH_CivilProfileProperties(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Profile Properties";

        return Value.ToString();
    }
}
