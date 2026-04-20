using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Alignment CANT information.
/// </summary>
public class GH_CivilCANTInfo : GH_Goo<CivilCANTInfo>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCANTInfo"/> class with no value.
    /// </summary>
    public GH_CivilCANTInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCANTInfo"/> class.
    /// </summary>
    /// <param name="cantInfo">The CANT info to wrap.</param>
    public GH_CivilCANTInfo(CivilCANTInfo cantInfo) : base(cantInfo)
    {
    }

    /// <summary>
    /// Initializes a new instance by copying another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilCANTInfo(GH_CivilCANTInfo other) : base(other.Value?.ShallowClone() as CivilCANTInfo)
    {
    }

    /// <summary>
    /// Constructs via the interface.
    /// </summary>
    public GH_CivilCANTInfo(ICivilCANTInfo cantInfo)
        : base((cantInfo as CivilCANTInfo)!)
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
                return "No CANT data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d CANT Info";

    /// <inheritdoc />
    public override string TypeDescription => "CANT (superelevation) information from a Civil 3D Alignment";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilCANTInfo(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilCANTInfo goo)
        {
            this.Value = goo.Value?.ShallowClone() as CivilCANTInfo;
            return true;
        }

        if (source is CivilCANTInfo wrapper)
        {
            this.Value = wrapper.ShallowClone() as CivilCANTInfo;
            return true;
        }

        if (source is ICivilCANTInfo props)
        {
            this.Value = (props as CivilCANTInfo)?.ShallowClone() as CivilCANTInfo;
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilCANTInfo)))
        {
            target = (Q)(object)this.Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilCANTInfo)))
        {
            target = (Q)(object)new GH_CivilCANTInfo(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null CANT Info";

        return this.Value.ToString();
    }
}
