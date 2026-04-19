using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Subassembly properties.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilSubassemblyPropertiesWrapper"/> containing
/// properties from a Subassembly.
/// </remarks>
public class GH_CivilSubassemblyProperties : GH_Goo<CivilSubassemblyPropertiesWrapper>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSubassemblyProperties"/> class with no value.
    /// </summary>
    public GH_CivilSubassemblyProperties()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSubassemblyProperties"/> class with the
    /// specified subassembly properties wrapper.
    /// </summary>
    /// <param name="subassemblyProperties">The Civil 3D subassembly properties wrapper.</param>
    public GH_CivilSubassemblyProperties(CivilSubassemblyPropertiesWrapper subassemblyProperties) : base(subassemblyProperties)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSubassemblyProperties"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilSubassemblyProperties(GH_CivilSubassemblyProperties other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilSubassemblyProperties"/> via the interface.
    /// </summary>
    public GH_CivilSubassemblyProperties(ICivilSubassemblyProperties subassemblyProperties)
        : base((subassemblyProperties as CivilSubassemblyPropertiesWrapper)!)
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
                return "No subassembly properties data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Subassembly Properties";

    /// <inheritdoc />
    public override string TypeDescription => "Properties from a Civil 3D Subassembly";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilSubassemblyProperties(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilSubassemblyProperties goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilSubassemblyPropertiesWrapper wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilSubassemblyProperties props)
        {
            Value = (props as CivilSubassemblyPropertiesWrapper)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilSubassemblyPropertiesWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilSubassemblyProperties)))
        {
            target = (Q)(object)new GH_CivilSubassemblyProperties(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Subassembly Properties";

        return Value.ToString();
    }
}
