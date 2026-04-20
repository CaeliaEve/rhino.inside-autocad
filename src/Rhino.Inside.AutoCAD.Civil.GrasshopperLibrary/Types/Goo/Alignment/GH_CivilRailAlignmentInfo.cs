using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D rail alignment information.
/// </summary>
public class GH_CivilRailAlignmentInfo : GH_Goo<CivilRailAlignmentInfo>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilRailAlignmentInfo"/> class with no value.
    /// </summary>
    public GH_CivilRailAlignmentInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilRailAlignmentInfo"/> class.
    /// </summary>
    /// <param name="railInfo">The rail alignment info to wrap.</param>
    public GH_CivilRailAlignmentInfo(CivilRailAlignmentInfo railInfo) : base(railInfo)
    {
    }

    /// <summary>
    /// Initializes a new instance by copying another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilRailAlignmentInfo(GH_CivilRailAlignmentInfo other) : base(other.Value?.ShallowClone() as CivilRailAlignmentInfo)
    {
    }

    /// <summary>
    /// Constructs via the interface.
    /// </summary>
    public GH_CivilRailAlignmentInfo(ICivilRailAlignmentInfo railInfo)
        : base((railInfo as CivilRailAlignmentInfo)!)
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
                return "No rail alignment data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Rail Alignment Info";

    /// <inheritdoc />
    public override string TypeDescription => "Rail alignment information from a Civil 3D Alignment";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilRailAlignmentInfo(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilRailAlignmentInfo goo)
        {
            this.Value = goo.Value?.ShallowClone() as CivilRailAlignmentInfo;
            return true;
        }

        if (source is CivilRailAlignmentInfo wrapper)
        {
            this.Value = wrapper.ShallowClone() as CivilRailAlignmentInfo;
            return true;
        }

        if (source is ICivilRailAlignmentInfo props)
        {
            this.Value = (props as CivilRailAlignmentInfo)?.ShallowClone() as CivilRailAlignmentInfo;
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilRailAlignmentInfo)))
        {
            target = (Q)(object)this.Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilRailAlignmentInfo)))
        {
            target = (Q)(object)new GH_CivilRailAlignmentInfo(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Rail Alignment Info";

        return this.Value.ToString();
    }
}
