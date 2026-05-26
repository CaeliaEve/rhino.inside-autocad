using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Subassembly properties.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilSubassemblyWrapper"/> containing
/// properties from a Subassembly.
/// </remarks>
public class GH_CivilSubassembly : GH_Goo<CivilSubassemblyWrapper>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSubassembly"/> class with no value.
    /// </summary>
    public GH_CivilSubassembly()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSubassembly"/> class with the
    /// specified subassembly properties wrapper.
    /// </summary>
    /// <param name="subassemblyWrapper">The Civil 3D subassembly properties wrapper.</param>
    public GH_CivilSubassembly(CivilSubassemblyWrapper subassemblyWrapper) : base(subassemblyWrapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSubassembly"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilSubassembly(GH_CivilSubassembly other) : base(other.Value?.ShallowClone())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilSubassembly"/> via the interface.
    /// </summary>
    public GH_CivilSubassembly(ICivilSubassembly subassemblyProperties)
        : base((subassemblyProperties as CivilSubassemblyWrapper)!)
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
        return new GH_CivilSubassembly(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilSubassembly goo)
        {
            Value = goo.Value?.ShallowClone();
            return true;
        }

        if (source is CivilSubassemblyWrapper wrapper)
        {
            Value = wrapper.ShallowClone();
            return true;
        }

        if (source is ICivilSubassembly props)
        {
            Value = (props as CivilSubassemblyWrapper)?.ShallowClone();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilSubassemblyWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilSubassembly)))
        {
            target = (Q)(object)new GH_CivilSubassembly(this);
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
