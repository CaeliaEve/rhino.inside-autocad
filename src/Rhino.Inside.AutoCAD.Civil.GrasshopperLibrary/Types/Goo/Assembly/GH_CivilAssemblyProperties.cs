using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Assembly properties.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilAssemblyProperties"/> containing
/// properties from an Assembly.
/// </remarks>
public class GH_CivilAssemblyProperties : GH_Goo<CivilAssemblyProperties>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAssemblyProperties"/> class with no value.
    /// </summary>
    public GH_CivilAssemblyProperties()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAssemblyProperties"/> class with the
    /// specified assembly properties wrapper.
    /// </summary>
    /// <param name="assemblyProperties">The Civil 3D assembly properties wrapper.</param>
    public GH_CivilAssemblyProperties(CivilAssemblyProperties assemblyProperties) : base(assemblyProperties)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAssemblyProperties"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilAssemblyProperties(GH_CivilAssemblyProperties other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilAssemblyProperties"/> via the interface.
    /// </summary>
    public GH_CivilAssemblyProperties(ICivilAssemblyProperties assemblyProperties)
        : base((assemblyProperties as CivilAssemblyProperties)!)
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
                return "No assembly properties data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Assembly Properties";

    /// <inheritdoc />
    public override string TypeDescription => "Properties from a Civil 3D Assembly";

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilAssemblyProperties(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilAssemblyProperties goo)
        {
            this.Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilAssemblyProperties wrapper)
        {
            this.Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilAssemblyProperties props)
        {
            this.Value = (props as CivilAssemblyProperties)?.Duplicate();
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilAssemblyProperties)))
        {
            target = (Q)(object)this.Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilAssemblyProperties)))
        {
            target = (Q)(object)new GH_CivilAssemblyProperties(this);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Civil3d Assembly Properties";

        return this.Value.ToString();
    }
}
