using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Corridor surfaces.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilCorridorSurfaceWrapper"/> containing
/// data from a Corridor surface with mesh preview capability.
/// </remarks>
public class GH_CivilCorridorSurface : GH_GeometricGoo<CivilCorridorSurfaceWrapper>, IGH_PreviewData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridorSurface"/> class with no value.
    /// </summary>
    public GH_CivilCorridorSurface()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridorSurface"/> class with the
    /// specified surface wrapper.
    /// </summary>
    /// <param name="surface">The Civil 3D corridor surface wrapper.</param>
    public GH_CivilCorridorSurface(CivilCorridorSurfaceWrapper surface) : base(surface)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridorSurface"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilCorridorSurface(GH_CivilCorridorSurface other) : base(other.Value?.ShallowClone())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilCorridorSurface"/> via the interface.
    /// </summary>
    public GH_CivilCorridorSurface(ICivilCorridorSurface surface)
        : base((surface as CivilCorridorSurfaceWrapper)!)
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
                return "No corridor surface data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Corridor Surface";

    /// <inheritdoc />
    public override string TypeDescription => "A surface from a Civil 3D Corridor";

    /// <inheritdoc />
    public override BoundingBox Boundingbox
    {
        get
        {
            if (Value?.Mesh == null)
                return BoundingBox.Empty;
            return Value.Mesh.GetBoundingBox(false);
        }
    }

    /// <inheritdoc />
    public BoundingBox ClippingBox => Boundingbox;

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilCorridorSurface(this);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo DuplicateGeometry()
    {
        return new GH_CivilCorridorSurface(this);
    }

    /// <inheritdoc />
    public override BoundingBox GetBoundingBox(Transform xform)
    {
        var box = Boundingbox;
        box.Transform(xform);
        return box;
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Transform(Transform xform)
    {
        if (Value == null)
            return this;

        var duplicated = Value.ShallowClone();
        duplicated.Mesh?.Transform(xform);
        return new GH_CivilCorridorSurface(duplicated);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        if (Value == null)
            return this;

        var duplicated = Value.ShallowClone();
        xmorph.Morph(duplicated.Mesh);
        return new GH_CivilCorridorSurface(duplicated);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilCorridorSurface goo)
        {
            Value = goo.Value?.ShallowClone();
            return true;
        }

        if (source is CivilCorridorSurfaceWrapper wrapper)
        {
            Value = wrapper.ShallowClone();
            return true;
        }

        if (source is ICivilCorridorSurface surface)
        {
            Value = (surface as CivilCorridorSurfaceWrapper)?.ShallowClone();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilCorridorSurfaceWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilCorridorSurface)))
        {
            target = (Q)(object)new GH_CivilCorridorSurface(this);
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_Mesh)) && Value?.Mesh != null)
        {
            target = (Q)(object)new GH_Mesh(Value.Mesh);
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(Mesh)) && Value?.Mesh != null)
        {
            target = (Q)(object)Value.Mesh;
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
        if (Value?.Mesh != null)
        {
            args.Pipeline.DrawMeshWires(Value.Mesh, args.Color, args.Thickness);
        }
    }

    /// <inheritdoc />
    public void DrawViewportMeshes(GH_PreviewMeshArgs args)
    {
        if (Value?.Mesh != null)
        {
            args.Pipeline.DrawMeshShaded(Value.Mesh, args.Material);
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Corridor Surface";

        return Value.ToString();
    }
}
