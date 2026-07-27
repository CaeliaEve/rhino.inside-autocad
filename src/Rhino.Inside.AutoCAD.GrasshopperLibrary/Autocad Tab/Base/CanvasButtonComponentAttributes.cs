using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Base attributes for components that show a single clickable button below their
/// capsule. Owns the layout, render and mouse handling for the button so derived
/// classes only declare when the button is visible; the button's caption and click
/// action are supplied at construction.
/// </summary>
/// <remarks>
/// Derived classes that also customise the component capsule (for example a palette
/// swap) override <see cref="RenderComponent"/> rather than
/// <see cref="Render"/>, which would bypass the button.
/// </remarks>
public abstract class CanvasButtonComponentAttributes : GH_ComponentAttributes
{
    private readonly ICanvasButton _button;

    /// <summary>
    /// Initializes a new instance of the <see cref="CanvasButtonComponentAttributes"/> class.
    /// </summary>
    /// <param name="owner">The owner component.</param>
    /// <param name="button">The button rendered below the component capsule.</param>
    protected CanvasButtonComponentAttributes(GH_Component owner, ICanvasButton button)
        : base(owner)
    {
        _button = button;
    }

    /// <summary>
    /// Gets a value indicating whether the button is currently shown on the component.
    /// Evaluated on every layout, render and mouse event, so it must be cheap and
    /// side-effect free.
    /// </summary>
    protected abstract bool IsButtonVisible { get; }

    /// <inheritdoc />
    protected override void Layout()
    {
        base.Layout();

        if (this.IsButtonVisible == false)
        {
            _button.ClearLayout();
            return;
        }

        // Add space for the button below the component
        var bounds = this.Bounds;
        bounds.Height += _button.Height;
        this.Bounds = bounds;

        _button.Layout(this.Bounds);
    }

    /// <inheritdoc />
    protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
    {
        this.RenderComponent(canvas, graphics, channel);

        if (channel != GH_CanvasChannel.Objects || this.IsButtonVisible == false)
            return;

        _button.Render(graphics, this.Selected, this.Owner.Locked);
    }

    /// <summary>
    /// Renders the owning component capsule. Override to customise the capsule
    /// appearance without reimplementing the button rendering.
    /// </summary>
    protected virtual void RenderComponent(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
    {
        base.Render(canvas, graphics, channel);
    }

    /// <inheritdoc />
    public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        if (this.IsButtonVisible)
        {
            var response = _button.RespondToMouseDown(sender, e);
            if (response != null) return response.Value;
        }

        return base.RespondToMouseDown(sender, e);
    }

    /// <inheritdoc />
    public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        // Delegated unconditionally so a pressed button always releases its mouse
        // capture, even if it was hidden between the press and the release.
        var response = _button.RespondToMouseUp(sender, e);
        if (response != null) return response.Value;

        return base.RespondToMouseUp(sender, e);
    }
}
