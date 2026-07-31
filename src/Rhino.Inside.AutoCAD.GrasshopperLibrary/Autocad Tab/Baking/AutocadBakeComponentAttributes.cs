using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Custom attributes for <see cref="AutocadBakeComponent"/> that displays
/// a "Bake" button when Driven Button is enabled.
/// </summary>
public class AutocadBakeComponentAttributes : CanvasButtonComponentAttributes
{
    private const string BakeButtonText = GrasshopperMessages.BakeButton;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutocadBakeComponentAttributes"/> class.
    /// </summary>
    public AutocadBakeComponentAttributes(AutocadBakeComponent owner)
        : base(owner, CreateBakeButton(owner))
    {
    }

    /// <summary>
    /// Creates the Bake button for the specified owner. A static factory because the
    /// button must be supplied to the base constructor.
    /// </summary>
    private static ICanvasButton CreateBakeButton(AutocadBakeComponent owner)
    {
        var bakeButton = new CanvasButton(BakeButtonText, owner.TriggerManualRun);

        return bakeButton;
    }

    private AutocadBakeComponent TypedOwner
        => (AutocadBakeComponent)this.Owner;

    /// <inheritdoc />
    protected override bool IsButtonVisible
        => this.TypedOwner.DrivenButtonEnabled;
}
