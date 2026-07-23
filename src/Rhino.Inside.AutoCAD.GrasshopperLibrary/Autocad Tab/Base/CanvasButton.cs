using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A clickable button rendered at the bottom of a component capsule. Composed by
/// component attributes classes to avoid duplicating the layout, render and mouse
/// handling logic for on-canvas buttons.
/// </summary>
public class CanvasButton
{
    private const int ButtonHeight = 22;
    private const int ButtonPadding = 3;

    private readonly string _text;
    private readonly Action _onClick;

    private RectangleF _bounds;
    private bool _pressed;

    /// <summary>
    /// Gets the total vertical space the button occupies, including padding. Component
    /// attributes should grow their bounds by this amount before calling <see cref="Layout"/>.
    /// </summary>
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

    /// <summary>
    /// Positions the button at the bottom of the specified component bounds. The bounds
    /// must already include the space reserved for the button (see <see cref="Height"/>).
    /// </summary>
    public void Layout(RectangleF componentBounds)
    {
        _bounds = new RectangleF(
            componentBounds.Left + ButtonPadding,
            componentBounds.Bottom - ButtonHeight - ButtonPadding,
            componentBounds.Width - ButtonPadding * 2,
            ButtonHeight);
    }

    /// <summary>
    /// Clears the button bounds so it no longer renders or responds to the mouse.
    /// Called when the button is hidden.
    /// </summary>
    public void ClearLayout()
    {
        _bounds = RectangleF.Empty;
    }

    /// <summary>
    /// Renders the button using a <see cref="GH_Capsule"/> for consistent Grasshopper styling.
    /// </summary>
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

    /// <summary>
    /// Handles a mouse down event. Returns the response for the owning attributes to
    /// forward, or null when the event was not over the button and the attributes
    /// should fall back to their base behaviour.
    /// </summary>
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

    /// <summary>
    /// Handles a mouse up event, invoking the click action when released over the button.
    /// Returns the response for the owning attributes to forward, or null when the button
    /// was not pressed and the attributes should fall back to their base behaviour.
    /// </summary>
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
