using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D offset alignment information.
/// </summary>
public class GH_CivilOffsetAlignmentInfo : GH_Goo<CivilOffsetAlignmentInfo>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilOffsetAlignmentInfo"/> class with no value.
    /// </summary>
    public GH_CivilOffsetAlignmentInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilOffsetAlignmentInfo"/> class.
    /// </summary>
    /// <param name="offsetInfo">The offset alignment info to wrap.</param>
    public GH_CivilOffsetAlignmentInfo(CivilOffsetAlignmentInfo offsetInfo) : base(offsetInfo)
    {
    }

    /// <summary>
    /// Initializes a new instance by copying another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilOffsetAlignmentInfo(GH_CivilOffsetAlignmentInfo other) : base(other.Value?.ShallowClone() as CivilOffsetAlignmentInfo)
    {
    }

    /// <summary>
    /// Constructs via the interface.
    /// </summary>
    public GH_CivilOffsetAlignmentInfo(ICivilOffsetAlignmentInfo offsetInfo)
        : base((offsetInfo as CivilOffsetAlignmentInfo)!)
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
                return "No offset alignment data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Offset Alignment Info";

    /// <inheritdoc />
    public override string TypeDescription => "Offset alignment information from a Civil 3D Alignment";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilOffsetAlignmentInfo(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilOffsetAlignmentInfo goo)
        {
            this.Value = goo.Value?.ShallowClone() as CivilOffsetAlignmentInfo;
            return true;
        }

        if (source is CivilOffsetAlignmentInfo wrapper)
        {
            this.Value = wrapper.ShallowClone() as CivilOffsetAlignmentInfo;
            return true;
        }

        if (source is ICivilOffsetAlignmentInfo props)
        {
            this.Value = (props as CivilOffsetAlignmentInfo)?.ShallowClone() as CivilOffsetAlignmentInfo;
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilOffsetAlignmentInfo)))
        {
            target = (Q)(object)this.Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilOffsetAlignmentInfo)))
        {
            target = (Q)(object)new GH_CivilOffsetAlignmentInfo(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Offset Alignment Info";

        return this.Value.ToString();
    }
}
