using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Custom attributes for record table components implementing <see cref="IStaleDataComponent"/>.
/// While the component's data is stale the capsule renders pale blue and a Refresh button
/// is displayed below the component. Components without a stale tracker (or with fresh
/// data) render entirely as standard. Selection, warning, error and locked states keep
/// their standard Grasshopper colours.
/// </summary>
public class RecordTable_ComponentAttributes : GH_ComponentAttributes
{
    /// <summary>
    /// The pale blue palette applied to the capsule while the component's data is stale.
    /// </summary>
    private static readonly GH_PaletteStyle StaleStyle = new(
        Color.FromArgb(185, 215, 240),
        Color.FromArgb(80, 110, 140),
        Color.Black);

    private readonly IStaleDataComponent _staleOwner;
    private readonly CanvasButton _refreshButton;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordTable_ComponentAttributes"/> class.
    /// </summary>
    /// <param name="owner">The owner component. Must implement <see cref="IStaleDataComponent"/>.</param>
    public RecordTable_ComponentAttributes(GH_Component owner)
        : base(owner)
    {
        _staleOwner = (IStaleDataComponent)owner;
        _refreshButton = new CanvasButton("Refresh", () => _staleOwner.StaleTracker?.Refresh());
    }

    /// <summary>
    /// True while the owner's data is stale. The tracker is read dynamically because it
    /// is composed after the attributes are constructed (in the component constructor).
    /// </summary>
    private bool IsStale => _staleOwner.StaleTracker?.IsStale == true;

    /// <inheritdoc />
    protected override void Layout()
    {
        base.Layout();

        if (this.IsStale)
        {
            // Add space for the Refresh button below the component
            var bounds = this.Bounds;
            bounds.Height += _refreshButton.Height;
            this.Bounds = bounds;

            _refreshButton.Layout(this.Bounds);
        }
        else
        {
            _refreshButton.ClearLayout();
        }
    }

    /// <inheritdoc />
    protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
    {
        if (channel != GH_CanvasChannel.Objects || this.IsStale == false)
        {
            base.Render(canvas, graphics, channel);
            return;
        }

        // Temporarily swap the standard palettes for the stale palette so the component
        // (and its message capsule) renders pale blue while stale. The selected, warning,
        // error and locked palettes are left untouched so those states take precedence.
        var normalStandard = GH_Skin.palette_normal_standard;
        var hiddenStandard = GH_Skin.palette_hidden_standard;

        try
        {
            GH_Skin.palette_normal_standard = StaleStyle;
            GH_Skin.palette_hidden_standard = StaleStyle;

            base.Render(canvas, graphics, channel);
        }
        finally
        {
            GH_Skin.palette_normal_standard = normalStandard;
            GH_Skin.palette_hidden_standard = hiddenStandard;
        }

        _refreshButton.Render(graphics, this.Selected, this.Owner.Locked);
    }

    /// <inheritdoc />
    public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        if (this.IsStale)
        {
            var response = _refreshButton.RespondToMouseDown(sender, e);
            if (response != null) return response.Value;
        }

        return base.RespondToMouseDown(sender, e);
    }

    /// <inheritdoc />
    public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        // Delegated unconditionally so a pressed button always releases its mouse capture.
        var response = _refreshButton.RespondToMouseUp(sender, e);
        if (response != null) return response.Value;

        return base.RespondToMouseUp(sender, e);
    }
}
