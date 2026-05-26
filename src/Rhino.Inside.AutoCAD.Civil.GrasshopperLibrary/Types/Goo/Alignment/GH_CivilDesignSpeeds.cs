using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Alignment design speeds information.
/// </summary>
public class GH_CivilDesignSpeeds : GH_Goo<CivilDesignSpeeds>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilDesignSpeeds"/> class with no value.
    /// </summary>
    public GH_CivilDesignSpeeds()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilDesignSpeeds"/> class.
    /// </summary>
    /// <param name="designSpeeds">The design speeds to wrap.</param>
    public GH_CivilDesignSpeeds(CivilDesignSpeeds designSpeeds) : base(designSpeeds)
    {
    }

    /// <summary>
    /// Initializes a new instance by copying another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilDesignSpeeds(GH_CivilDesignSpeeds other) : base(other.Value?.ShallowClone() as CivilDesignSpeeds)
    {
    }

    /// <summary>
    /// Constructs via the interface.
    /// </summary>
    public GH_CivilDesignSpeeds(ICivilDesignSpeeds designSpeeds)
        : base((designSpeeds as CivilDesignSpeeds)!)
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
                return "No design speeds data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Design Speeds";

    /// <inheritdoc />
    public override string TypeDescription => "Design speed information from a Civil 3D Alignment";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilDesignSpeeds(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilDesignSpeeds goo)
        {
            this.Value = goo.Value?.ShallowClone() as CivilDesignSpeeds;
            return true;
        }

        if (source is CivilDesignSpeeds wrapper)
        {
            this.Value = wrapper.ShallowClone() as CivilDesignSpeeds;
            return true;
        }

        if (source is ICivilDesignSpeeds props)
        {
            this.Value = (props as CivilDesignSpeeds)?.ShallowClone() as CivilDesignSpeeds;
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilDesignSpeeds)))
        {
            target = (Q)(object)this.Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilDesignSpeeds)))
        {
            target = (Q)(object)new GH_CivilDesignSpeeds(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Design Speeds";

        return this.Value.ToString();
    }
}
