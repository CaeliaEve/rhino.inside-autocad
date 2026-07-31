using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// A clickable button rendered at the bottom of a component capsule. Component
/// attributes compose one of these rather than duplicating the layout, render and
/// mouse handling logic for on-canvas buttons.
/// </summary>
/// <remarks>
/// The mouse handlers return a nullable response: a value means the button consumed
/// the event and the owning attributes should return it; <c>null</c> means the event
/// was not the button's, and the attributes should fall through to their base
/// implementation.
/// </remarks>
public interface ICanvasButton
{
    /// <summary>
    /// Gets the total vertical space the button occupies, including padding. Component
    /// attributes must grow their bounds by this amount before calling <see cref="Layout"/>.
    /// </summary>
    float Height { get; }

    /// <summary>
    /// Positions the button at the bottom of the specified component bounds. The bounds
    /// must already include the space reserved for the button (see <see cref="Height"/>).
    /// </summary>
    /// <param name="componentBounds">The bounds of the owning component capsule.</param>
    void Layout(RectangleF componentBounds);

    /// <summary>
    /// Clears the button bounds so it no longer renders or responds to the mouse.
    /// Called when the button is hidden.
    /// </summary>
    void ClearLayout();

    /// <summary>
    /// Renders the button. Does nothing when <see cref="Layout"/> has not positioned it.
    /// </summary>
    /// <param name="graphics">The canvas graphics to draw into.</param>
    /// <param name="selected">Whether the owning component is selected.</param>
    /// <param name="locked">Whether the owning component is locked.</param>
    void Render(Graphics graphics, bool selected, bool locked);

    /// <summary>
    /// Handles a mouse down event.
    /// </summary>
    /// <returns>
    /// The response for the owning attributes to forward, or null when the event was
    /// not over the button and the attributes should fall back to their base behaviour.
    /// </returns>
    GH_ObjectResponse? RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e);

    /// <summary>
    /// Handles a mouse up event, invoking the click action when released over the button.
    /// </summary>
    /// <returns>
    /// The response for the owning attributes to forward, or null when the button was
    /// not pressed and the attributes should fall back to their base behaviour.
    /// </returns>
    GH_ObjectResponse? RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e);
}
