using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <inheritdoc cref="ICanvasButton"/>
public class CanvasButton : ICanvasButton
{
    private const int ButtonHeight = CanvasButtonConstants.ButtonHeight;
    private const int ButtonPadding = CanvasButtonConstants.ButtonPadding;

    private readonly string _text;
    private readonly Action _onClick;

    private RectangleF _bounds;
    private bool _pressed;

    /// <inheritdoc />
    public float Height => ButtonHeight + ButtonPadding * 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="CanvasButton"/> class.
    /// </summary>
    /// <param name="text">The text displayed on the button.</param>
    /// <param name="onClick">The action invoked when the button is clicked.</param>
    public CanvasButton(string text, Action onClick)
    {
        _text = text;
        _onClick = onClick;
    }

    /// <inheritdoc />
    public void Layout(RectangleF componentBounds)
    {
        _bounds = new RectangleF(
            componentBounds.Left + ButtonPadding,
            componentBounds.Bottom - ButtonHeight - ButtonPadding,
            componentBounds.Width - ButtonPadding * 2,
            ButtonHeight);
    }

    /// <inheritdoc />
    public void ClearLayout()
    {
        _bounds = RectangleF.Empty;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Rendered as a <see cref="GH_Capsule"/> for consistent Grasshopper styling.
    /// </remarks>
    public void Render(Graphics graphics, bool selected, bool locked)
    {
        // Guard against invalid button bounds (can happen if Layout hasn't been called yet)
        if (_bounds.Width <= 0 || _bounds.Height <= 0)
            return;

        var capsule = GH_Capsule.CreateTextCapsule(
            Rectangle.Round(_bounds),
            Rectangle.Round(_bounds),
            _pressed ? GH_Palette.Grey : GH_Palette.Black,
            _text,
            GH_FontServer.Standard,
            GH_Orientation.horizontal_center,
            2,
            10);

        capsule.Render(graphics, selected, locked, false);
        capsule.Dispose();
    }

    /// <inheritdoc />
    public GH_ObjectResponse? RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        if (e.Button == System.Windows.Forms.MouseButtons.Left &&
            _bounds.Contains(e.CanvasLocation))
        {
            _pressed = true;
            sender.Invalidate();
            return GH_ObjectResponse.Capture;
        }

        return null;
    }

    /// <inheritdoc />
    public GH_ObjectResponse? RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        if (_pressed == false)
            return null;

        _pressed = false;

        if (_bounds.Contains(e.CanvasLocation))
        {
            _onClick();
        }

        sender.Invalidate();
        return GH_ObjectResponse.Release;
    }
}
