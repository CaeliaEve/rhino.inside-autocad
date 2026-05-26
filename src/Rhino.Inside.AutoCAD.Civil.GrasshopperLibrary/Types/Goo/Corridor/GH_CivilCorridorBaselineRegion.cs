using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Corridor baseline regions.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilCorridorBaselineRegionWrapper"/> containing
/// data from a Corridor baseline region.
/// </remarks>
public class GH_CivilCorridorBaselineRegion : GH_Goo<CivilCorridorBaselineRegionWrapper>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridorBaselineRegion"/> class with no value.
    /// </summary>
    public GH_CivilCorridorBaselineRegion()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridorBaselineRegion"/> class with the
    /// specified baseline region wrapper.
    /// </summary>
    /// <param name="region">The Civil 3D corridor baseline region wrapper.</param>
    public GH_CivilCorridorBaselineRegion(CivilCorridorBaselineRegionWrapper region) : base(region)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridorBaselineRegion"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilCorridorBaselineRegion(GH_CivilCorridorBaselineRegion other) : base(other.Value?.ShallowClone())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilCorridorBaselineRegion"/> via the interface.
    /// </summary>
    public GH_CivilCorridorBaselineRegion(ICivilCorridorBaselineRegion region)
        : base((region as CivilCorridorBaselineRegionWrapper)!)
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
                return "No corridor baseline region data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Corridor Baseline Region";

    /// <inheritdoc />
    public override string TypeDescription => "A baseline region from a Civil 3D Corridor";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilCorridorBaselineRegion(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilCorridorBaselineRegion goo)
        {
            Value = goo.Value?.ShallowClone();
            return true;
        }

        if (source is CivilCorridorBaselineRegionWrapper wrapper)
        {
            Value = wrapper.ShallowClone();
            return true;
        }

        if (source is ICivilCorridorBaselineRegion region)
        {
            Value = (region as CivilCorridorBaselineRegionWrapper)?.ShallowClone();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilCorridorBaselineRegionWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilCorridorBaselineRegion)))
        {
            target = (Q)(object)new GH_CivilCorridorBaselineRegion(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Corridor Baseline Region";

        return Value.ToString();
    }
}
