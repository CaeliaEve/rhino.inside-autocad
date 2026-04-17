using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Sites.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilSiteWrapper"/> containing
/// site information and collections.
/// </remarks>
public class GH_CivilSite : GH_Goo<CivilSiteWrapper>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSite"/> class with no value.
    /// </summary>
    public GH_CivilSite()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSite"/> class with the
    /// specified site wrapper.
    /// </summary>
    /// <param name="site">The Civil 3D site wrapper.</param>
    public GH_CivilSite(CivilSiteWrapper site) : base(site)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSite"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilSite(GH_CivilSite other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilSite"/> via the interface.
    /// </summary>
    public GH_CivilSite(ICivilSite site)
        : base((site as CivilSiteWrapper)!)
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
                return "No site data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Site";

    /// <inheritdoc />
    public override string TypeDescription => "A Civil 3D Site container";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilSite(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilSite goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilSiteWrapper wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilSite site)
        {
            Value = (site as CivilSiteWrapper)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilSiteWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilSite)))
        {
            target = (Q)(object)new GH_CivilSite(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Site";

        return Value.ToString();
    }
}
