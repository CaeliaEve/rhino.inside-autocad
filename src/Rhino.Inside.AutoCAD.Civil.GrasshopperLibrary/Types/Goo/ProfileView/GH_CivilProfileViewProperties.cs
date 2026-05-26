using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D ProfileView properties.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilProfileViewProperties"/> containing
/// properties from a ProfileView.
/// </remarks>
public class GH_CivilProfileViewProperties : GH_Goo<CivilProfileViewProperties>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileViewProperties"/> class with no value.
    /// </summary>
    public GH_CivilProfileViewProperties()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileViewProperties"/> class with the
    /// specified ProfileView properties wrapper.
    /// </summary>
    /// <param name="profileViewProperties">The Civil 3D ProfileView properties wrapper.</param>
    public GH_CivilProfileViewProperties(CivilProfileViewProperties profileViewProperties) : base(profileViewProperties)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileViewProperties"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilProfileViewProperties(GH_CivilProfileViewProperties other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilProfileViewProperties"/> via the interface.
    /// </summary>
    public GH_CivilProfileViewProperties(ICivilProfileViewProperties profileViewProperties)
        : base((profileViewProperties as CivilProfileViewProperties)!)
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
                return "No ProfileView properties data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d ProfileView Properties";

    /// <inheritdoc />
    public override string TypeDescription => "Properties from a Civil 3D ProfileView";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilProfileViewProperties(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilProfileViewProperties goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilProfileViewProperties wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilProfileViewProperties props)
        {
            Value = (props as CivilProfileViewProperties)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilProfileViewProperties)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilProfileViewProperties)))
        {
            target = (Q)(object)new GH_CivilProfileViewProperties(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d ProfileView Properties";

        return Value.ToString();
    }
}
