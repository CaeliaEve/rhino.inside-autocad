using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for a Civil 3D station point (station + elevation).
/// </summary>
public class GH_CivilStationPoint : GH_Goo<CivilStationPoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilStationPoint"/> class with no value.
    /// </summary>
    public GH_CivilStationPoint()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilStationPoint"/> class.
    /// </summary>
    /// <param name="stationPoint">The station point to wrap.</param>
    public GH_CivilStationPoint(CivilStationPoint stationPoint) : base(stationPoint)
    {
    }

    /// <summary>
    /// Initializes a new instance by copying another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilStationPoint(GH_CivilStationPoint other) : base(other.Value)
    {
    }

    /// <summary>
    /// Constructs via the interface.
    /// </summary>
    public GH_CivilStationPoint(ICivilStationPoint stationPoint)
        : base((stationPoint as CivilStationPoint)!)
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
                return "No station point data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Station Point";

    /// <inheritdoc />
    public override string TypeDescription => "A station value with its corresponding elevation";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilStationPoint(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilStationPoint goo)
        {
            this.Value = goo.Value;
            return true;
        }

        if (source is CivilStationPoint wrapper)
        {
            this.Value = wrapper;
            return true;
        }

        if (source is ICivilStationPoint props)
        {
            this.Value = props as CivilStationPoint;
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilStationPoint)))
        {
            target = (Q)(object)this.Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilStationPoint)))
        {
            target = (Q)(object)new GH_CivilStationPoint(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Station Point";

        return this.Value.ToString();
    }
}
