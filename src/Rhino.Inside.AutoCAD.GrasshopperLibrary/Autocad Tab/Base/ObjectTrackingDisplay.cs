using Grasshopper;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <inheritdoc cref="IObjectTrackingDisplay"/>
public class ObjectTrackingDisplay : IObjectTrackingDisplay
{
    private const string TrackingSingleObjectMessage = GrasshopperMessages.TrackingSingleObject;
    private const string TrackingObjectsMessageFormat = GrasshopperMessages.TrackingObjectsFormat;

    private readonly ITrackedObjectsComponent _owner;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectTrackingDisplay"/> class.
    /// </summary>
    /// <param name="owner">The component whose display is kept in sync.</param>
    public ObjectTrackingDisplay(ITrackedObjectsComponent owner)
    {
        _owner = owner;
    }

    /// <inheritdoc />
    public void Update()
    {
        var count = _owner.TrackedObjectCount;
        var message = count switch
        {
            <= 0 => null,
            1 => TrackingSingleObjectMessage,
            _ => string.Format(TrackingObjectsMessageFormat, count)
        };

        // Count changes always change the message, so this also gates repaints
        if (_owner.Message == message)
            return;

        _owner.Message = message;

        // The message capsule size changed - expire the layout and repaint. During a
        // solution Grasshopper redraws afterwards anyway; this covers out-of-solution
        // changes (context menu Forget Connections, file load).
        _owner.Attributes?.ExpireLayout();
        Instances.ActiveCanvas?.Invalidate();
    }
}
