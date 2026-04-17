using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D alignment line sub-entities.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilAlignmentLineWrapper"/> and provides
/// preview support for displaying the line geometry in viewports.
/// </remarks>
public class GH_CivilAlignmentLine : GH_GeometricGoo<CivilAlignmentLineWrapper>, IGH_PreviewData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentLine"/> class with no value.
    /// </summary>
    public GH_CivilAlignmentLine()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentLine"/> class with the
    /// specified alignment line wrapper.
    /// </summary>
    /// <param name="alignmentLine">The Civil 3D alignment line wrapper.</param>
    public GH_CivilAlignmentLine(CivilAlignmentLineWrapper alignmentLine) : base(alignmentLine)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentLine"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilAlignmentLine(GH_CivilAlignmentLine other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilAlignmentLine"/> via the interface.
    /// </summary>
    public GH_CivilAlignmentLine(ICivilAlignmentLine alignmentLine)
        : base((alignmentLine as CivilAlignmentLineWrapper)!)
    {
    }

    /// <inheritdoc />
    public override bool IsValid => Value != null && Value.Line.IsValid;

    /// <inheritdoc />
    public override string IsValidWhyNot
    {
        get
        {
            if (Value == null)
                return "No alignment line data";
            if (!Value.Line.IsValid)
                return "Invalid line geometry";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Alignment Line";

    /// <inheritdoc />
    public override string TypeDescription => "A line sub-entity from a Civil 3D Alignment";

    /// <inheritdoc />
    public override BoundingBox Boundingbox
    {
        get
        {
            if (Value == null)
                return BoundingBox.Empty;

            return Value.Line.BoundingBox;
        }
    }

    /// <inheritdoc />
    public BoundingBox ClippingBox => Boundingbox;

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilAlignmentLine(this);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo DuplicateGeometry()
    {
        return new GH_CivilAlignmentLine(this);
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

        var transformedLine = Value.Line;
        transformedLine.Transform(xform);

        var transformed = new CivilAlignmentLineWrapper(
            transformedLine,
            Value.StartStation,
            Value.EndStation,
            Value.Length,
            Value.Index);

        return new GH_CivilAlignmentLine(transformed);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        if (Value == null)
            return this;

        // Lines can only be morphed by transforming endpoints
        var startPt = xmorph.MorphPoint(Value.Line.From);
        var endPt = xmorph.MorphPoint(Value.Line.To);
        var morphedLine = new Line(startPt, endPt);

        var morphed = new CivilAlignmentLineWrapper(
            morphedLine,
            Value.StartStation,
            Value.EndStation,
            morphedLine.Length,
            Value.Index);

        return new GH_CivilAlignmentLine(morphed);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilAlignmentLine goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilAlignmentLineWrapper wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilAlignmentLine alignmentLine)
        {
            Value = (alignmentLine as CivilAlignmentLineWrapper)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilAlignmentLineWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilAlignmentLine)))
        {
            target = (Q)(object)new GH_CivilAlignmentLine(this);
            return true;
        }

        // Cast to GH_Line
        if (typeof(Q).IsAssignableFrom(typeof(GH_Line)) && Value != null)
        {
            target = (Q)(object)new GH_Line(Value.Line);
            return true;
        }

        // Cast to Line
        if (typeof(Q).IsAssignableFrom(typeof(Line)) && Value != null)
        {
            target = (Q)(object)Value.Line;
            return true;
        }

        // Cast to GH_Curve
        if (typeof(Q).IsAssignableFrom(typeof(GH_Curve)) && Value != null)
        {
            target = (Q)(object)new GH_Curve(new LineCurve(Value.Line));
            return true;
        }

        // Cast to Curve
        if (typeof(Q).IsAssignableFrom(typeof(Curve)) && Value != null)
        {
            target = (Q)(object)new LineCurve(Value.Line);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
        if (Value == null)
            return;

        args.Pipeline.DrawLine(Value.Line, args.Color, args.Thickness);
    }

    /// <inheritdoc />
    public void DrawViewportMeshes(GH_PreviewMeshArgs args)
    {
        // Lines are drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Alignment Line";

        return Value.ToString();
    }
}
