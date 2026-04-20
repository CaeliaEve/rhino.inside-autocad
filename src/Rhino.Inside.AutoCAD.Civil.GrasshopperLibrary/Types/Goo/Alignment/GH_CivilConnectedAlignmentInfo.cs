using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D connected alignment information.
/// </summary>
public class GH_CivilConnectedAlignmentInfo : GH_Goo<CivilConnectedAlignmentInfo>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilConnectedAlignmentInfo"/> class with no value.
    /// </summary>
    public GH_CivilConnectedAlignmentInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilConnectedAlignmentInfo"/> class.
    /// </summary>
    /// <param name="connectedInfo">The connected alignment info to wrap.</param>
    public GH_CivilConnectedAlignmentInfo(CivilConnectedAlignmentInfo connectedInfo) : base(connectedInfo)
    {
    }

    /// <summary>
    /// Initializes a new instance by copying another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilConnectedAlignmentInfo(GH_CivilConnectedAlignmentInfo other) : base(other.Value?.ShallowClone() as CivilConnectedAlignmentInfo)
    {
    }

    /// <summary>
    /// Constructs via the interface.
    /// </summary>
    public GH_CivilConnectedAlignmentInfo(ICivilConnectedAlignmentInfo connectedInfo)
        : base((connectedInfo as CivilConnectedAlignmentInfo)!)
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
                return "No connected alignment data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Connected Alignment Info";

    /// <inheritdoc />
    public override string TypeDescription => "Connected alignment information from a Civil 3D Alignment";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilConnectedAlignmentInfo(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilConnectedAlignmentInfo goo)
        {
            this.Value = goo.Value?.ShallowClone() as CivilConnectedAlignmentInfo;
            return true;
        }

        if (source is CivilConnectedAlignmentInfo wrapper)
        {
            this.Value = wrapper.ShallowClone() as CivilConnectedAlignmentInfo;
            return true;
        }

        if (source is ICivilConnectedAlignmentInfo props)
        {
            this.Value = (props as CivilConnectedAlignmentInfo)?.ShallowClone() as CivilConnectedAlignmentInfo;
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilConnectedAlignmentInfo)))
        {
            target = (Q)(object)this.Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilConnectedAlignmentInfo)))
        {
            target = (Q)(object)new GH_CivilConnectedAlignmentInfo(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Connected Alignment Info";

        return this.Value.ToString();
    }
}
