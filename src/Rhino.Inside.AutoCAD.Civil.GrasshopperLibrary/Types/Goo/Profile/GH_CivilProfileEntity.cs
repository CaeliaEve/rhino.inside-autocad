using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D profile entities.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilProfileEntityWrapper"/> and provides
/// preview support for displaying the entity curve in viewports.
/// </remarks>
public class GH_CivilProfileEntity : GH_GeometricGoo<CivilProfileEntityWrapper>, IGH_PreviewData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileEntity"/> class with no value.
    /// </summary>
    public GH_CivilProfileEntity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileEntity"/> class with the
    /// specified entity wrapper.
    /// </summary>
    /// <param name="entity">The Civil 3D profile entity wrapper.</param>
    public GH_CivilProfileEntity(CivilProfileEntityWrapper entity) : base(entity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileEntity"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilProfileEntity(GH_CivilProfileEntity other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilProfileEntity"/> via the interface.
    /// </summary>
    public GH_CivilProfileEntity(ICivilProfileEntity entity)
        : base((entity as CivilProfileEntityWrapper)!)
    {
    }

    /// <inheritdoc />
    public override bool IsValid => Value?.Curve != null && Value.Curve.IsValid;

    /// <inheritdoc />
    public override string IsValidWhyNot
    {
        get
        {
            if (Value == null)
                return "No profile entity data";
            if (Value.Curve == null || !Value.Curve.IsValid)
                return "Invalid entity geometry";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Profile Entity";

    /// <inheritdoc />
    public override string TypeDescription => "An entity (Tangent, CircularArc, Parabola) from a Civil 3D Profile";

    /// <inheritdoc />
    public override BoundingBox Boundingbox
    {
        get
        {
            if (Value?.Curve == null)
                return BoundingBox.Empty;

            return Value.Curve.GetBoundingBox(true);
        }
    }

    /// <inheritdoc />
    public BoundingBox ClippingBox => Boundingbox;

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilProfileEntity(this);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo DuplicateGeometry()
    {
        return new GH_CivilProfileEntity(this);
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
        if (Value?.Curve == null)
            return this;

        var transformedCurve = Value.Curve.DuplicateCurve();
        transformedCurve.Transform(xform);

        var transformed = new CivilProfileEntityWrapper(
            Value.EntityType,
            Value.StartStation,
            Value.EndStation,
            Value.StartElevation,
            Value.EndElevation,
            Value.Length,
            Value.EntityIndex,
            transformedCurve);

        return new GH_CivilProfileEntity(transformed);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        if (Value?.Curve == null)
            return this;

        var morphedCurve = Value.Curve.DuplicateCurve();
        xmorph.Morph(morphedCurve);

        var morphed = new CivilProfileEntityWrapper(
            Value.EntityType,
            Value.StartStation,
            Value.EndStation,
            Value.StartElevation,
            Value.EndElevation,
            Value.Length,
            Value.EntityIndex,
            morphedCurve);

        return new GH_CivilProfileEntity(morphed);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilProfileEntity goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilProfileEntityWrapper wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilProfileEntity entity)
        {
            Value = (entity as CivilProfileEntityWrapper)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilProfileEntityWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilProfileEntity)))
        {
            target = (Q)(object)new GH_CivilProfileEntity(this);
            return true;
        }

        // Cast to GH_Curve
        if (typeof(Q).IsAssignableFrom(typeof(GH_Curve)) && Value?.Curve != null)
        {
            target = (Q)(object)new GH_Curve(Value.Curve.DuplicateCurve());
            return true;
        }

        // Cast to Curve
        if (typeof(Q).IsAssignableFrom(typeof(Curve)) && Value?.Curve != null)
        {
            target = (Q)(object)Value.Curve.DuplicateCurve();
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
        if (Value?.Curve == null)
            return;

        args.Pipeline.DrawCurve(Value.Curve, args.Color, args.Thickness);
    }

    /// <inheritdoc />
    public void DrawViewportMeshes(GH_PreviewMeshArgs args)
    {
        // Profile entities are drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Profile Entity";

        return Value.ToString();
    }
}
