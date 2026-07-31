using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Custom attributes for components implementing <see cref="ITrackedObjectsComponent"/>.
/// Renders the component capsule black while the component is tracking objects in the
/// host document, mirroring Rhino.Inside.Revit's tracked-component appearance.
/// Selection, warning, error and locked states keep their standard Grasshopper colours.
/// </summary>
public class ObjectTrackingComponentAttributes : GH_ComponentAttributes
{
    private readonly ITrackedObjectsComponent _trackedOwner;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectTrackingComponentAttributes"/> class.
    /// </summary>
    /// <param name="owner">The owner component. Must implement <see cref="ITrackedObjectsComponent"/>.</param>
    public ObjectTrackingComponentAttributes(GH_Component owner)
        : base(owner)
    {
        _trackedOwner = (ITrackedObjectsComponent)owner;
    }

    /// <inheritdoc />
    protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
    {
        if (channel != GH_CanvasChannel.Objects || _trackedOwner.TrackedObjectCount <= 0)
        {
            base.Render(canvas, graphics, channel);
            return;
        }

        // Temporarily swap the standard palettes for the black palette so the component
        // (and its message capsule) renders black while tracking. The selected, warning,
        // error and locked palettes are left untouched so those states take precedence.
        var normalStandard = GH_Skin.palette_normal_standard;
        var hiddenStandard = GH_Skin.palette_hidden_standard;

        try
        {
            GH_Skin.palette_normal_standard = GH_Skin.palette_black_standard;
            GH_Skin.palette_hidden_standard = GH_Skin.palette_black_standard;

            base.Render(canvas, graphics, channel);
        }
        finally
        {
            GH_Skin.palette_normal_standard = normalStandard;
            GH_Skin.palette_hidden_standard = hiddenStandard;
        }
    }
}
