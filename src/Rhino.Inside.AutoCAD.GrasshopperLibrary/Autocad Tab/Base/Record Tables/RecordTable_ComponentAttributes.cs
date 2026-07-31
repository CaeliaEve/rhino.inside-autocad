using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Custom attributes for record table components implementing <see cref="IStaleDataComponent"/>.
/// While the component's data is stale the capsule renders pale blue and a Refresh button
/// is displayed below the component. Components without a stale tracker (or with fresh
/// data) render entirely as standard. Selection, warning, error and locked states keep
/// their standard Grasshopper colours.
/// </summary>
public class RecordTable_ComponentAttributes : CanvasButtonComponentAttributes
{
    private const string RefreshButtonText = GrasshopperMessages.RefreshButton;

    /// <summary>
    /// The pale blue palette applied to the capsule while the component's data is stale.
    /// </summary>
    private static readonly GH_PaletteStyle StaleStyle = new(
        Color.FromArgb(185, 215, 240),
        Color.FromArgb(80, 110, 140),
        Color.Black);

    private readonly IStaleDataComponent _staleOwner;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordTable_ComponentAttributes"/> class.
    /// </summary>
    /// <param name="owner">The owner component. Must implement <see cref="IStaleDataComponent"/>.</param>
    public RecordTable_ComponentAttributes(GH_Component owner)
        : base(owner, CreateRefreshButton(owner))
    {
        _staleOwner = (IStaleDataComponent)owner;
    }

    /// <summary>
    /// Creates the Refresh button for the specified owner. A static factory because the
    /// button must be supplied to the base constructor.
    /// </summary>
    private static ICanvasButton CreateRefreshButton(GH_Component owner)
    {
        var staleOwner = (IStaleDataComponent)owner;

        // The tracker is resolved on click rather than now, because it is composed
        // after the attributes are constructed (in the component constructor).
        var refreshButton = new CanvasButton(
            RefreshButtonText,
            () => staleOwner.StaleTracker?.Refresh());

        return refreshButton;
    }

    /// <summary>
    /// True while the owner's data is stale. The tracker is read dynamically because it
    /// is composed after the attributes are constructed (in the component constructor).
    /// </summary>
    protected override bool IsButtonVisible => _staleOwner.StaleTracker?.IsStale == true;

    /// <inheritdoc />
    protected override void RenderComponent(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
    {
        if (channel != GH_CanvasChannel.Objects || this.IsButtonVisible == false)
        {
            base.RenderComponent(canvas, graphics, channel);
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

            base.RenderComponent(canvas, graphics, channel);
        }
        finally
        {
            GH_Skin.palette_normal_standard = normalStandard;
            GH_Skin.palette_hidden_standard = hiddenStandard;
        }
    }
}
