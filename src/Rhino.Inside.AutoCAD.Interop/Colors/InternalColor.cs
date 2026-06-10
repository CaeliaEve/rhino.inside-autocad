using Rhino.Inside.AutoCAD.Core.Interfaces;
using CadColor = Autodesk.AutoCAD.Colors.Color;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Represents an AutoCAD color, supporting RGB, ByLayer, ByBlock, and ACI modes.
/// </summary>
/// <remarks>
/// AutoCAD uses special color index values for ByLayer (256) and ByBlock (0).
/// This class provides methods to create and manipulate colors with these
/// special values, as well as standard RGB and ACI (AutoCAD Color Index) colors.
/// </remarks>
public class InternalColor : IColor
{
    /// <inheritdoc/>
    public byte Red { get; }

    /// <inheritdoc/>
    public byte Green { get; }

    /// <inheritdoc/>
    public byte Blue { get; }

    /// <inheritdoc/>
    public byte Alpha { get; }

    /// <summary>
    /// Constructs a new <see cref="InternalColor"/> from <see cref="Autodesk.AutoCAD.Colors.Color"/>.
    /// </summary>
    public InternalColor(CadColor color)
    {
        this.Red = color.ColorValue.R;
        this.Green = color.ColorValue.G;
        this.Blue = color.ColorValue.B;
        this.Alpha = color.ColorValue.A;
    }

    /// <inheritdoc/>
    public bool IsEqualTo(IColor other)
    {
        return this.Red == other.Red
               && this.Green == other.Green
               && this.Blue == other.Blue
               && this.Alpha == other.Alpha;
    }
}
