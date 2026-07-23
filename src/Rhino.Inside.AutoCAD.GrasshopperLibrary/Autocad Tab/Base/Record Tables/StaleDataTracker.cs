using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Composable stale-data tracking behaviour for reference components. Owns the stale
/// state, the accumulation of document changes, the component display updates (message,
/// runtime remark, canvas invalidation), the Auto Update context menu item and its
/// persistence. Components opt in by holding an instance and exposing it through
/// <see cref="IStaleDataComponent.StaleTracker"/>; no inheritance required.
/// </summary>
public class StaleDataTracker : IStaleDataTracker
{
    private const string AutoUpdateEnabledKey = "AutoUpdateEnabled";

    private readonly GH_Component _owner;
    private readonly Func<IDbObject, bool> _isTrackedObject;

    private bool _autoUpdateEnabled;

    // Ids are accumulated per change kind so repeated events for the same object
    // (AutoCAD raises ObjectModified multiple times per command) count once, and so
    // counts keep accumulating across successive commands while the component is stale.
    private readonly HashSet<long> _addedIds = new();
    private readonly HashSet<long> _deletedIds = new();
    private readonly HashSet<long> _modifiedIds = new();

    /// <inheritdoc />
    public bool AutoUpdateEnabled => _autoUpdateEnabled;

    /// <inheritdoc />
    public bool IsStale => _addedIds.Count > 0 || _deletedIds.Count > 0 || _modifiedIds.Count > 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="StaleDataTracker"/> class.
    /// </summary>
    /// <param name="owner">The component whose data is tracked.</param>
    /// <param name="isTrackedObject">
    /// A predicate returning true for changed objects that affect the owner's data
    /// (typically a type check on the unwrapped AutoCAD object).
    /// </param>
    public StaleDataTracker(GH_Component owner, Func<IDbObject, bool> isTrackedObject)
    {
        _owner = owner;
        _isTrackedObject = isTrackedObject;
    }

    /// <inheritdoc />
    public bool NotifyDocumentChanged(IAutocadDocumentChange change)
    {
        // Wired reference params represent upstream data becoming invalid, so they still
        // expire the component; the solve then clears any stale state.
        // Input params ignore modifications (side effects like reference count updates).
        foreach (var ghParam in _owner.Params.Input.OfType<IReferenceParam>())
        {
            if (ghParam.NeedsToBeExpired(change, includeModified: false)) return true;
        }

        foreach (var ghParam in _owner.Params.Output.OfType<IReferenceParam>())
        {
            if (ghParam.NeedsToBeExpired(change)) return true;
        }

        var hasNewChanges = false;

        hasNewChanges |= this.Accumulate(change, ChangeType.ObjectCreated, _addedIds);
        hasNewChanges |= this.Accumulate(change, ChangeType.ObjectErased, _deletedIds);
        hasNewChanges |= this.Accumulate(change, ChangeType.ObjectModified, _modifiedIds);

        if (hasNewChanges)
            this.UpdateStaleDisplay();

        return false;
    }

    /// <inheritdoc />
    public void Refresh()
    {
        // Clear immediately so the visuals reset even before the solve completes.
        this.ClearStaleState();

        _owner.ExpireSolution(true);
    }

    /// <summary>
    /// Clears the stale state at the start of a solve. Called from the owner's
    /// BeforeSolveInstance so every path that re-solves the component (the Refresh
    /// button, the Auto Update toggle, upstream expiry, a manual recompute) resets it.
    /// </summary>
    public void OnSolveBeginning()
    {
        this.ClearStaleState();
    }

    /// <summary>
    /// Appends the Auto Update menu item to the owner's context menu.
    /// </summary>
    public void AppendMenuItems(ToolStripDropDown menu)
    {
        GH_DocumentObject.Menu_AppendSeparator(menu);

        var autoUpdateItem = GH_DocumentObject.Menu_AppendItem(
            menu,
            "Auto Update",
            this.OnAutoUpdateMenuClick,
            true,
            _autoUpdateEnabled
        );
        autoUpdateItem.ToolTipText = "When enabled, the component automatically updates when the AutoCAD document changes. " +
                                     "When disabled, document changes mark the data as stale until Refresh is pressed.";
    }

    /// <summary>
    /// Reads the Auto Update setting. Missing keys (files saved before this feature)
    /// fall back to disabled, keeping stale tracking as the default behaviour.
    /// </summary>
    public void Read(GH_IReader reader)
    {
        _autoUpdateEnabled = false;
        reader.TryGetBoolean(AutoUpdateEnabledKey, ref _autoUpdateEnabled);
    }

    /// <summary>
    /// Writes the Auto Update setting.
    /// </summary>
    public void Write(GH_IWriter writer)
    {
        writer.SetBoolean(AutoUpdateEnabledKey, _autoUpdateEnabled);
    }

    /// <summary>
    /// Handles the click event for the Auto Update menu item.
    /// </summary>
    private void OnAutoUpdateMenuClick(object? sender, EventArgs e)
    {
        _autoUpdateEnabled = !_autoUpdateEnabled;

        if (_autoUpdateEnabled)
        {
            // Re-solving brings a stale component back in sync immediately.
            this.Refresh();
            return;
        }

        Instances.ActiveCanvas?.Invalidate();
    }

    /// <summary>
    /// Adds the tracked objects of the given change type to the id set, returning true
    /// when any object was not already accumulated.
    /// </summary>
    private bool Accumulate(IAutocadDocumentChange change, ChangeType changeType, HashSet<long> ids)
    {
        var hasNewChanges = false;

        foreach (var dbObject in change.GetAffectedObjects(changeType))
        {
            if (_isTrackedObject(dbObject) == false) continue;

            hasNewChanges |= ids.Add(dbObject.Id.Value);
        }

        return hasNewChanges;
    }

    /// <summary>
    /// Updates the owner's display to reflect the stale state: the "Stale Data" message
    /// capsule, a runtime remark describing what changed, and a canvas repaint so the
    /// stale colour and Refresh button appear.
    /// </summary>
    private void UpdateStaleDisplay()
    {
        _owner.Message = "Stale Data";

        // Messages from the previous solve describe outdated data; replace them with
        // the stale remark. The next solve clears runtime messages automatically.
        _owner.ClearRuntimeMessages();
        _owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, this.GetStaleDescription());

        _owner.Attributes?.ExpireLayout();
        Instances.ActiveCanvas?.Invalidate();
    }

    /// <summary>
    /// Returns a description of the accumulated changes, for example
    /// "Data is stale: 2 added, 1 deleted, 3 modified. Press Refresh to update."
    /// </summary>
    private string GetStaleDescription()
    {
        var parts = new List<string>();

        if (_addedIds.Count > 0) parts.Add($"{_addedIds.Count} added");
        if (_deletedIds.Count > 0) parts.Add($"{_deletedIds.Count} deleted");

        // An object that was also added or deleted this session reads as that change,
        // not as an additional modification.
        var modifiedCount = _modifiedIds.Except(_addedIds).Except(_deletedIds).Count();
        if (modifiedCount > 0) parts.Add($"{modifiedCount} modified");

        return $"Data is stale: {string.Join(", ", parts)}. Press Refresh to update.";
    }

    /// <summary>
    /// Clears the accumulated changes and resets the owner's stale display.
    /// </summary>
    private void ClearStaleState()
    {
        if (this.IsStale == false) return;

        _addedIds.Clear();
        _deletedIds.Clear();
        _modifiedIds.Clear();

        _owner.Message = null;

        _owner.Attributes?.ExpireLayout();
        Instances.ActiveCanvas?.Invalidate();
    }
}
