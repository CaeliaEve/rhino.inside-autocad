using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D volume properties.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilTinVolumeSurfaceProperties"/> containing
/// volume statistics from a TIN Volume Surface.
/// </remarks>
public class GH_CivilVolumeProperties : GH_Goo<CivilTinVolumeSurfaceProperties>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilVolumeProperties"/> class with no value.
    /// </summary>
    public GH_CivilVolumeProperties()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilVolumeProperties"/> class with the
    /// specified volume properties wrapper.
    /// </summary>
    /// <param name="tinVolumeSurfaceProperties">The Civil 3D volume properties wrapper.</param>
    public GH_CivilVolumeProperties(CivilTinVolumeSurfaceProperties tinVolumeSurfaceProperties) : base(tinVolumeSurfaceProperties)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilVolumeProperties"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilVolumeProperties(GH_CivilVolumeProperties other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilVolumeProperties"/> via the interface.
    /// </summary>
    public GH_CivilVolumeProperties(ICivilTinVolumeSurfaceProperties volumeProperties)
        : base((volumeProperties as CivilTinVolumeSurfaceProperties)!)
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
                return "No volume properties data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Volume Properties";

    /// <inheritdoc />
    public override string TypeDescription => "Volume statistics from a Civil 3D TIN Volume Surface";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilVolumeProperties(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilVolumeProperties goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilTinVolumeSurfaceProperties wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilTinVolumeSurfaceProperties props)
        {
            Value = (props as CivilTinVolumeSurfaceProperties)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilTinVolumeSurfaceProperties)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilVolumeProperties)))
        {
            target = (Q)(object)new GH_CivilVolumeProperties(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Volume Properties";

        return Value.ToString();
    }
}
