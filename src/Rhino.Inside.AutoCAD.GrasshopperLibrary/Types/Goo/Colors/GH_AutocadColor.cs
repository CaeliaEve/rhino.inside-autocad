using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Grasshopper Goo wrapper for AutoCAD colors with ByLayer/ByBlock support.
/// </summary>
/// <remarks>
/// This Goo type supports AutoCAD's special color modes:
/// <list type="bullet">
/// <item><description>ByLayer (ColorIndex=256) - color inherited from layer</description></item>
/// <item><description>ByBlock (ColorIndex=0) - color inherited from containing block</description></item>
/// <item><description>ACI (ColorIndex=1-255) - AutoCAD Color Index colors</description></item>
/// <item><description>RGB - true color values</description></item>
/// </list>
/// </remarks>
public class GH_AutocadColor : GH_Goo<IAutocadColor>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_AutocadColor"/> class with no value.
    /// </summary>
    public GH_AutocadColor()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_AutocadColor"/> class with the
    /// specified AutoCAD color wrapper.
    /// </summary>
    /// <param name="wrapper">The AutoCAD color wrapper to wrap.</param>
    public GH_AutocadColor(IAutocadColor wrapper) : base(wrapper)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_AutocadColor"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_AutocadColor(GH_AutocadColor other)
    {
        this.Value = other.Value;
    }

    /// <inheritdoc />
    public override bool IsValid => this.Value != null;

    /// <inheritdoc />
    public override string TypeName => "AutoCAD Color";

    /// <inheritdoc />
    public override string TypeDescription =>
        "Represents an AutoCAD color (RGB, ByLayer, ByBlock, or ACI index)";

    /// <inheritdoc />
    public override IGH_Goo Duplicate() => new GH_AutocadColor(this);

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        switch (source)
        {
            case GH_AutocadColor goo:
                this.Value = goo.Value;
                return true;

            case AutocadColorWrapper wrapper:
                this.Value = wrapper;
                return true;

            case InternalColor wrapper:
                this.Value = AutocadColorWrapper.CreateFromRgb(
                    wrapper.Red, wrapper.Green, wrapper.Blue);
                return true;

            case GH_Colour ghColor:
                var c = ghColor.Value;
                this.Value = AutocadColorWrapper.CreateFromRgb(c.R, c.G, c.B);
                return true;

            case Color drawingColor:
                this.Value = AutocadColorWrapper.CreateFromRgb(
                    drawingColor.R, drawingColor.G, drawingColor.B);
                return true;

            case GH_Integer ghInt:
                this.Value = AutocadColorWrapper.CreateFromIndex((short)ghInt.Value);
                return true;

            case GH_Number ghNum:
                this.Value = AutocadColorWrapper.CreateFromIndex((short)ghNum.Value);
                return true;

            case int intVal:
                this.Value = AutocadColorWrapper.CreateFromIndex((short)intVal);
                return true;

            default:
                return false;
        }
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (this.Value == null) return false;

        if (typeof(Q).IsAssignableFrom(typeof(AutocadColorWrapper)))
        {
            target = (Q)(object)this.Value;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(InternalColor)))
        {
            var internalColor = new InternalColor(this.Value.Unwrap());
            target = (Q)(object)internalColor;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_AutocadColor)))
        {
            target = (Q)(object)new GH_AutocadColor(this.Value);
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_Colour)))
        {
            var approxColor = this.Value.Unwrap().ColorValue;

            var color = Color.FromArgb(approxColor.A, approxColor.R, approxColor.G, approxColor.B);

            target = (Q)(object)new GH_Colour(color);
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(Color)))
        {
            var approxColor = this.Value.Unwrap().ColorValue;

            var color = Color.FromArgb(approxColor.A, approxColor.R, approxColor.G, approxColor.B);
            target = (Q)(object)color;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_Integer)))
        {
            target = (Q)(object)new GH_Integer(this.Value.ColorIndex);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null) return "Null AutoCAD Color";
        if (this.Value.IsByLayer) return "AutoCAD Color [ByLayer]";
        if (this.Value.IsByBlock) return "AutoCAD Color [ByBlock]";

        var trueColor = this.Value.Unwrap().ColorValue;

        return $"AutoCAD Color [RGB({trueColor.R},{trueColor.G},{trueColor.B}), ACI:{this.Value.ColorIndex}]";
    }
}
