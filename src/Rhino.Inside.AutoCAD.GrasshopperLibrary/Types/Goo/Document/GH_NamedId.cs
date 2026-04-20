using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for a NamedId.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="NamedId"/> combining a display name with an AutoCAD ObjectId.
/// Used for references to named Civil 3D objects like Sites, Styles, and DesignCheckSets.
/// </remarks>
public class GH_NamedId : GH_Goo<NamedId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_NamedId"/> class with no value.
    /// </summary>
    public GH_NamedId()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_NamedId"/> class with the
    /// specified NamedId.
    /// </summary>
    /// <param name="namedId">The NamedId to wrap.</param>
    public GH_NamedId(NamedId namedId) : base(namedId)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_NamedId"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_NamedId(GH_NamedId other) : base(other.Value?.ShallowClone() as NamedId)
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_NamedId"/> via the interface.
    /// </summary>
    /// <param name="namedId">The INamedId interface to wrap.</param>
    public GH_NamedId(INamedId namedId)
        : base((namedId as NamedId)!)
    {
    }

    /// <inheritdoc />
    public override bool IsValid => this.Value is { IsValid: true };

    /// <inheritdoc />
    public override string IsValidWhyNot
    {
        get
        {
            if (this.Value == null)
                return "No NamedId data";
            if (!this.Value.IsValid)
                return "The referenced ObjectId is not valid";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Named Id";

    /// <inheritdoc />
    public override string TypeDescription => "A name combined with an AutoCAD ObjectId reference";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_NamedId(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_NamedId goo)
        {
            this.Value = goo.Value?.ShallowClone() as NamedId;
            return true;
        }

        if (source is NamedId namedId)
        {
            this.Value = namedId.ShallowClone() as NamedId;
            return true;
        }

        if (source is INamedId iNamedId)
        {
            this.Value = (iNamedId as NamedId)?.ShallowClone() as NamedId;
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(NamedId)))
        {
            target = (Q)(object)this.Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_NamedId)))
        {
            target = (Q)(object)new GH_NamedId(this);
            return true;
        }

        // Allow casting to GH_AutocadObjectId
        if (typeof(Q).IsAssignableFrom(typeof(GH_AutocadObjectId)) && this.Value?.ObjectId != null)
        {
            target = (Q)(object)new GH_AutocadObjectId(this.Value.ObjectId);
            return true;
        }

        // Allow casting to string (name)
        if (typeof(Q).IsAssignableFrom(typeof(GH_String)) && this.Value != null)
        {
            target = (Q)(object)new GH_String(this.Value.Name);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Named Id";

        return this.Value.ToString();
    }
}
