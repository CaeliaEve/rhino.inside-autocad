namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Tracks selection and deselection of objects in a Grasshopper document,
/// since Grasshopper does not provide a native selection changed event.
/// </summary>
public interface IGrasshopperSelectionTracker : IDisposable
{
    /// <summary>Raised when one or more objects are selected.</summary>
    event EventHandler<IGrasshopperSelectionEventArgs> ObjectsSelected;

    /// <summary>Raised when one or more objects are deselected.</summary>
    event EventHandler<IGrasshopperSelectionEventArgs> ObjectsDeselected;
}