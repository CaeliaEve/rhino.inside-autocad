using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Alignment reference station information.
/// </summary>
public class GH_CivilReferenceStation : GH_Goo<CivilReferenceStation>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilReferenceStation"/> class with no value.
    /// </summary>
    public GH_CivilReferenceStation()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilReferenceStation"/> class.
    /// </summary>
    /// <param name="referenceStation">The reference station to wrap.</param>
    public GH_CivilReferenceStation(CivilReferenceStation referenceStation) : base(referenceStation)
    {
    }

    /// <summary>
    /// Initializes a new instance by copying another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilReferenceStation(GH_CivilReferenceStation other) : base(other.Value?.ShallowClone() as CivilReferenceStation)
    {
    }

    /// <summary>
    /// Constructs via the interface.
    /// </summary>
    public GH_CivilReferenceStation(ICivilReferenceStation referenceStation)
        : base((referenceStation as CivilReferenceStation)!)
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
                return "No reference station data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Reference Station";

    /// <inheritdoc />
    public override string TypeDescription => "Reference station information from a Civil 3D Alignment";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilReferenceStation(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilReferenceStation goo)
        {
            this.Value = goo.Value?.ShallowClone() as CivilReferenceStation;
            return true;
        }

        if (source is CivilReferenceStation wrapper)
        {
            this.Value = wrapper.ShallowClone() as CivilReferenceStation;
            return true;
        }

        if (source is ICivilReferenceStation props)
        {
            this.Value = (props as CivilReferenceStation)?.ShallowClone() as CivilReferenceStation;
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilReferenceStation)))
        {
            target = (Q)(object)this.Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilReferenceStation)))
        {
            target = (Q)(object)new GH_CivilReferenceStation(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Reference Station";

        return this.Value.ToString();
    }
}
