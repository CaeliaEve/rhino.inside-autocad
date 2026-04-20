using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Alignment properties.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilAlignmentProperties"/> containing
/// properties from an Alignment.
/// </remarks>
public class GH_CivilAlignmentProperties : GH_Goo<CivilAlignmentProperties>
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
    public GH_CivilAlignmentProperties(CivilAlignmentProperties alignmentProperties) : base(alignmentProperties)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentProperties"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilAlignmentProperties(GH_CivilAlignmentProperties other) : base(other.Value?.ShallowClone())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilAlignmentProperties"/> via the interface.
    /// </summary>
    public GH_CivilAlignmentProperties(ICivilAlignmentProperties alignmentProperties)
        : base((alignmentProperties as CivilAlignmentProperties)!)
    {
    }

    /// <inheritdoc />
    public override bool IsValid => this.Value != null;

    /// <inheritdoc />
    public override string IsValidWhyNot
    {
        get
        {
            if (this.Value == null)
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
            this.Value = goo.Value?.ShallowClone();
            return true;
        }

        if (source is CivilAlignmentProperties wrapper)
        {
            this.Value = wrapper.ShallowClone();
            return true;
        }

        if (source is ICivilAlignmentProperties props)
        {
            this.Value = (props as CivilAlignmentProperties)?.ShallowClone();
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilAlignmentProperties)))
        {
            target = (Q)(object)this.Value!;
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
        if (this.Value == null)
            return "Null Civil3d Alignment Properties";

        return this.Value.ToString();
    }
}
