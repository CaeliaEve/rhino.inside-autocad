using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Parcel properties.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilParcelProperties"/> containing
/// properties from a Parcel.
/// </remarks>
public class GH_CivilParcelProperties : GH_Goo<CivilParcelProperties>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilParcelProperties"/> class with no value.
    /// </summary>
    public GH_CivilParcelProperties()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilParcelProperties"/> class with the
    /// specified parcel properties wrapper.
    /// </summary>
    /// <param name="parcelProperties">The Civil 3D parcel properties wrapper.</param>
    public GH_CivilParcelProperties(CivilParcelProperties parcelProperties) : base(parcelProperties)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilParcelProperties"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilParcelProperties(GH_CivilParcelProperties other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilParcelProperties"/> via the interface.
    /// </summary>
    public GH_CivilParcelProperties(ICivilParcelProperties parcelProperties)
        : base((parcelProperties as CivilParcelProperties)!)
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
                return "No parcel properties data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Parcel Properties";

    /// <inheritdoc />
    public override string TypeDescription => "Properties from a Civil 3D Parcel";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilParcelProperties(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilParcelProperties goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilParcelProperties wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilParcelProperties props)
        {
            Value = (props as CivilParcelProperties)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilParcelProperties)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilParcelProperties)))
        {
            target = (Q)(object)new GH_CivilParcelProperties(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Parcel Properties";

        return Value.ToString();
    }
}
