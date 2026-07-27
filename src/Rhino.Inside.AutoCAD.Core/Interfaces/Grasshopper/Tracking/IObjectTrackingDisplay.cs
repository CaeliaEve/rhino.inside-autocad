namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Keeps a tracking component's on-canvas display (message capsule and capsule colour)
/// in sync with its tracked-object count.
/// </summary>
/// <seealso cref="ITrackedObjectsComponent"/>
public interface IObjectTrackingDisplay
{
    /// <summary>
    /// Updates the owner's message capsule to reflect the current tracked-object count
    /// and repaints the canvas. Safe to call during or outside a solution; does nothing
    /// when the message is already up to date.
    /// </summary>
    void Update();
}
