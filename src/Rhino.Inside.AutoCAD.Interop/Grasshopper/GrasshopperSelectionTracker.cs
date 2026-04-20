using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using System.Windows.Forms;

namespace Rhino.Inside.AutoCAD.Interop;

/// <inheritdoc cref="IGrasshopperSelectionTracker"/>
public class GrasshopperSelectionTracker : IGrasshopperSelectionTracker
{

    private readonly GH_Document _document;
    private HashSet<Guid> _previouslySelected = new HashSet<Guid>();
    private bool _disposed;

    /// <inheritdoc />
    public event EventHandler<IGrasshopperSelectionEventArgs> ObjectsSelected;

    /// <inheritdoc />
    public event EventHandler<IGrasshopperSelectionEventArgs> ObjectsDeselected;

    /// <summary>
    /// Creates a new tracker for the given document and begins listening
    /// to canvas input events.
    /// </summary>
    public GrasshopperSelectionTracker(GH_Document document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));

        var canvas = Grasshopper.Instances.ActiveCanvas;
        if (canvas == null) return;

        canvas.MouseUp += this.OnCanvasInput;
        canvas.KeyUp += this.OnCanvasKeyUp;
    }

    private void OnCanvasInput(object sender, MouseEventArgs e)
        => this.CheckSelectionChanged();

    private void OnCanvasKeyUp(object sender, KeyEventArgs e)
        => this.CheckSelectionChanged();

    /// <summary>
    /// Compares the current selection against the last known selection
    /// and raises events for anything that changed.
    /// </summary>
    private void CheckSelectionChanged()
    {
        if (_disposed || _document == null) return;

        var currentlySelected = new HashSet<Guid>(
            _document.SelectedObjects().Select(o => o.InstanceGuid));

        var newlySelected = currentlySelected.Except(_previouslySelected).ToList();
        var newlyDeselected = _previouslySelected.Except(currentlySelected).ToList();

        if (newlySelected.Count > 0)
        {
            var objects = this.ResolveObjects(newlySelected);
            ObjectsSelected?.Invoke(this, new GrasshopperSelectionEventArgs(objects));
        }

        if (newlyDeselected.Count > 0)
        {
            var objects = this.ResolveObjects(newlyDeselected);
            ObjectsDeselected?.Invoke(this, new GrasshopperSelectionEventArgs(objects));
        }

        _previouslySelected = currentlySelected;
    }

    private List<IGH_DocumentObject> ResolveObjects(IEnumerable<Guid> guids)
    {
        return guids
            .Select(guid => _document.FindObject(guid, topLevelOnly: true))
            .Where(obj => obj != null)
            .ToList();
    }

    /// <summary>
    /// Unsubscribes from all canvas events.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        var canvas = Grasshopper.Instances.ActiveCanvas;
        if (canvas == null) return;

        canvas.MouseUp -= this.OnCanvasInput;
        canvas.KeyUp -= this.OnCanvasKeyUp;
    }
}