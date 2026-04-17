using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D ProfileView bands.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilProfileViewBandWrapper"/> containing
/// band information from a ProfileView.
/// </remarks>
public class GH_CivilProfileViewBand : GH_Goo<CivilProfileViewBandWrapper>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileViewBand"/> class with no value.
    /// </summary>
    public GH_CivilProfileViewBand()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileViewBand"/> class with the
    /// specified ProfileView band wrapper.
    /// </summary>
    /// <param name="profileViewBand">The Civil 3D ProfileView band wrapper.</param>
    public GH_CivilProfileViewBand(CivilProfileViewBandWrapper profileViewBand) : base(profileViewBand)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileViewBand"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilProfileViewBand(GH_CivilProfileViewBand other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilProfileViewBand"/> via the interface.
    /// </summary>
    public GH_CivilProfileViewBand(ICivilProfileViewBand profileViewBand)
        : base((profileViewBand as CivilProfileViewBandWrapper)!)
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
                return "No ProfileView band data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d ProfileView Band";

    /// <inheritdoc />
    public override string TypeDescription => "A band from a Civil 3D ProfileView";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilProfileViewBand(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilProfileViewBand goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilProfileViewBandWrapper wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilProfileViewBand band)
        {
            Value = (band as CivilProfileViewBandWrapper)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilProfileViewBandWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilProfileViewBand)))
        {
            target = (Q)(object)new GH_CivilProfileViewBand(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d ProfileView Band";

        return Value.ToString();
    }
}
