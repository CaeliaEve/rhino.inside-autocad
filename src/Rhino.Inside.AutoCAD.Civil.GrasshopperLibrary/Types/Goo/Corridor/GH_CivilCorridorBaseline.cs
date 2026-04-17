using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Corridor baselines.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilCorridorBaselineWrapper"/> containing
/// data from a Corridor baseline.
/// </remarks>
public class GH_CivilCorridorBaseline : GH_Goo<CivilCorridorBaselineWrapper>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridorBaseline"/> class with no value.
    /// </summary>
    public GH_CivilCorridorBaseline()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridorBaseline"/> class with the
    /// specified baseline wrapper.
    /// </summary>
    /// <param name="baseline">The Civil 3D corridor baseline wrapper.</param>
    public GH_CivilCorridorBaseline(CivilCorridorBaselineWrapper baseline) : base(baseline)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridorBaseline"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilCorridorBaseline(GH_CivilCorridorBaseline other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilCorridorBaseline"/> via the interface.
    /// </summary>
    public GH_CivilCorridorBaseline(ICivilCorridorBaseline baseline)
        : base((baseline as CivilCorridorBaselineWrapper)!)
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
                return "No corridor baseline data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Corridor Baseline";

    /// <inheritdoc />
    public override string TypeDescription => "A baseline from a Civil 3D Corridor";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilCorridorBaseline(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilCorridorBaseline goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilCorridorBaselineWrapper wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilCorridorBaseline baseline)
        {
            Value = (baseline as CivilCorridorBaselineWrapper)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilCorridorBaselineWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilCorridorBaseline)))
        {
            target = (Q)(object)new GH_CivilCorridorBaseline(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Corridor Baseline";

        return Value.ToString();
    }
}
