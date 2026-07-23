using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Extension methods that keep a tracking component's on-canvas display (message capsule
/// and capsule colour) in sync with its tracked-object count.
/// </summary>
public static class ObjectTrackingDisplayExtensions
{
    /// <summary>
    /// Updates the component's message capsule to reflect the current tracked-object count
    /// and repaints the canvas. Safe to call during or outside a solution; does nothing
    /// when the message is already up to date.
    /// </summary>
    /// <param name="component">The component whose display should be updated.</param>
    public static void UpdateTrackingDisplay<T>(this T component)
        where T : GH_Component, ITrackedObjectsComponent
    {
        var count = component.TrackedObjectCount;
        var message = count switch
        {
            <= 0 => null,
            1 => "Tracking 1 Object",
            _ => $"Tracking {count} Objects"
        };

        // Count changes always change the message, so this also gates repaints
        if (component.Message == message)
            return;

        component.Message = message;

        // The message capsule size changed - expire the layout and repaint. During a
        // solution Grasshopper redraws afterwards anyway; this covers out-of-solution
        // changes (context menu Clear Connection, file load).
        component.Attributes?.ExpireLayout();
        Instances.ActiveCanvas?.Invalidate();
    }
}
