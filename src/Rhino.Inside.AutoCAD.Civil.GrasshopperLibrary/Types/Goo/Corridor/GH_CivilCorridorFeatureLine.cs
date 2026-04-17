using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Corridor feature lines.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilCorridorFeatureLineWrapper"/> containing
/// data from a Corridor feature line with curve preview capability.
/// </remarks>
public class GH_CivilCorridorFeatureLine : GH_GeometricGoo<CivilCorridorFeatureLineWrapper>, IGH_PreviewData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridorFeatureLine"/> class with no value.
    /// </summary>
    public GH_CivilCorridorFeatureLine()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridorFeatureLine"/> class with the
    /// specified feature line wrapper.
    /// </summary>
    /// <param name="featureLine">The Civil 3D corridor feature line wrapper.</param>
    public GH_CivilCorridorFeatureLine(CivilCorridorFeatureLineWrapper featureLine) : base(featureLine)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridorFeatureLine"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilCorridorFeatureLine(GH_CivilCorridorFeatureLine other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilCorridorFeatureLine"/> via the interface.
    /// </summary>
    public GH_CivilCorridorFeatureLine(ICivilCorridorFeatureLine featureLine)
        : base((featureLine as CivilCorridorFeatureLineWrapper)!)
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
                return "No corridor feature line data";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Corridor Feature Line";

    /// <inheritdoc />
    public override string TypeDescription => "A feature line from a Civil 3D Corridor";

    /// <inheritdoc />
    public override BoundingBox Boundingbox
    {
        get
        {
            if (Value?.Curve == null)
                return BoundingBox.Empty;
            return Value.Curve.GetBoundingBox(false);
        }
    }

    /// <inheritdoc />
    public BoundingBox ClippingBox => Boundingbox;

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilCorridorFeatureLine(this);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo DuplicateGeometry()
    {
        return new GH_CivilCorridorFeatureLine(this);
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

        var duplicated = Value.Duplicate();
        duplicated.Curve?.Transform(xform);
        return new GH_CivilCorridorFeatureLine(duplicated);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        if (Value == null)
            return this;

        var duplicated = Value.Duplicate();
        xmorph.Morph(duplicated.Curve);
        return new GH_CivilCorridorFeatureLine(duplicated);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilCorridorFeatureLine goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilCorridorFeatureLineWrapper wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilCorridorFeatureLine featureLine)
        {
            Value = (featureLine as CivilCorridorFeatureLineWrapper)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilCorridorFeatureLineWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilCorridorFeatureLine)))
        {
            target = (Q)(object)new GH_CivilCorridorFeatureLine(this);
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_Curve)) && Value?.Curve != null)
        {
            target = (Q)(object)new GH_Curve(Value.Curve);
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(Curve)) && Value?.Curve != null)
        {
            target = (Q)(object)Value.Curve;
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
        if (Value?.Curve != null)
        {
            args.Pipeline.DrawCurve(Value.Curve, args.Color, args.Thickness);
        }
    }

    /// <inheritdoc />
    public void DrawViewportMeshes(GH_PreviewMeshArgs args)
    {
        // Feature lines are curves, drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Corridor Feature Line";

        return Value.ToString();
    }
}
