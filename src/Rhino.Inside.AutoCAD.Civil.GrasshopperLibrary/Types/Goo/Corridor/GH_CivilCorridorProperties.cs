using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Corridor properties.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilCorridorPropertiesWrapper"/> containing
/// properties from a Corridor.
/// </remarks>
public class GH_CivilCorridorProperties : GH_Goo<CivilCorridorPropertiesWrapper>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridorProperties"/> class with no value.
    /// </summary>
    public GH_CivilCorridorProperties()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridorProperties"/> class with the
    /// specified corridor properties wrapper.
    /// </summary>
    /// <param name="corridorProperties">The Civil 3D corridor properties wrapper.</param>
    public GH_CivilCorridorProperties(CivilCorridorPropertiesWrapper corridorProperties) : base(corridorProperties)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridorProperties"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilCorridorProperties(GH_CivilCorridorProperties other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilCorridorProperties"/> via the interface.
    /// </summary>
    public GH_CivilCorridorProperties(ICivilCorridorProperties corridorProperties)
        : base((corridorProperties as CivilCorridorPropertiesWrapper)!)
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
                return "No corridor properties data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Corridor Properties";

    /// <inheritdoc />
    public override string TypeDescription => "Properties from a Civil 3D Corridor";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilCorridorProperties(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilCorridorProperties goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilCorridorPropertiesWrapper wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilCorridorProperties props)
        {
            Value = (props as CivilCorridorPropertiesWrapper)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilCorridorPropertiesWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilCorridorProperties)))
        {
            target = (Q)(object)new GH_CivilCorridorProperties(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Corridor Properties";

        return Value.ToString();
    }
}
