using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Rhino.Commands;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Base class for Grasshopper components that create Civil 3D objects.
/// Provides optional "Replace" functionality that tracks all created objects
/// and automatically deletes them before creating new ones when the solver re-runs.
/// Supports multi-input components where SolveInstance is called N times for N inputs.
/// </summary>
/// <remarks>
/// <para><b>Rhino Undo/Redo Protection</b></para>
/// <para>
/// This class implements protection against a crash that occurs when Civil 3D objects
/// are created immediately after a Rhino undo/redo operation. The issue manifests as an
/// <see cref="System.AccessViolationException"/> inside <c>AeccDbMgd.dll</c> (Civil 3D's
/// managed database layer) when calling <c>Alignment.Create()</c> or similar Civil 3D
/// object creation methods.
/// </para>
/// <para><b>Root Cause</b></para>
/// <para>
/// When the user performs an undo in Rhino (e.g., undoing a curve move), the Grasshopper
/// solver is triggered to re-run. The solver receives the <c>IsEndUndo</c> event and runs
/// immediately after, but Civil 3D's internal state is still unstable at this point.
/// Even though our transaction state appears clean (0 active transactions, successful
/// document lock), Civil 3D's native layer crashes when attempting to create new objects.
/// </para>
/// <para><b>Solution</b></para>
/// <para>
/// The fix is to defer object creation by one solve cycle after undo/redo completes:
/// <list type="number">
///   <item><description><c>IsBeginUndo</c> fires → Set <c>_undoInProgress = true</c></description></item>
///   <item><description><c>IsEndUndo</c> fires → Set <c>_justFinishedUndo = true</c></description></item>
///   <item><description><c>BeforeSolveInstance</c> runs → Detect <c>_justFinishedUndo</c>, skip this solve, schedule re-solve in 100ms</description></item>
///   <item><description>Re-solve runs → Civil 3D is now stable, object creation succeeds</description></item>
/// </list>
/// </para>
/// <para>
/// Tracked objects are preserved during skip because Civil 3D objects are not affected by
/// Rhino undo - they remain valid and should be deleted when the deferred solve runs (if
/// Replace mode is enabled).
/// </para>
/// <para>
/// Derived classes must call <see cref="ShouldSkipSolve"/> at the start of their
/// <c>SolveInstance</c> method and return early if it returns <c>true</c>.
/// </para>
/// </remarks>
public abstract class RhinoInsideAutocad_CreateComponentBase : RhinoInsideAutocad_ComponentBase
{
    // Serialization keys
    private const string ReplaceEnabledKey = "ReplaceEnabled";

    // State tracking
    private bool _replaceEnabled = true;
    private readonly List<IObjectId> _lastCreatedObjectIds = new();
    private bool _deletionPerformedThisCycle = false;
    private bool _isSubscribedToUndo = false;

    // Static flags shared across all instances - undo affects all components
    private static bool _undoInProgress = false;
    private static bool _justFinishedUndo = false;

    // Per-instance state for current solve cycle
    private bool _skipThisSolve = false;

    /// <summary>
    /// Constructs a new instance of the <see cref="RhinoInsideAutocad_CreateComponentBase"/> class.
    /// </summary>
    protected RhinoInsideAutocad_CreateComponentBase(
        string name,
        string nickname,
        string description,
        string category,
        string subCategory) : base(name, nickname, description, category, subCategory)
    {
    }

    /// <inheritdoc />
    public override void AddedToDocument(GH_Document document)
    {
        base.AddedToDocument(document);
        SubscribeToUndoRedo();
    }

    /// <inheritdoc />
    public override void RemovedFromDocument(GH_Document document)
    {
        UnsubscribeFromUndoRedo();
        base.RemovedFromDocument(document);
    }

    private void SubscribeToUndoRedo()
    {
        if (!_isSubscribedToUndo)
        {
            Command.UndoRedo += OnRhinoUndoRedo;
            _isSubscribedToUndo = true;
        }
    }

    private void UnsubscribeFromUndoRedo()
    {
        if (_isSubscribedToUndo)
        {
            Command.UndoRedo -= OnRhinoUndoRedo;
            _isSubscribedToUndo = false;
        }
    }

    private void OnRhinoUndoRedo(object? sender, UndoRedoEventArgs e)
    {
        if (e.IsBeginUndo || e.IsBeginRedo)
        {
            // Set flag BEFORE solver runs
            _undoInProgress = true;
            // DON'T clear tracked objects - Civil 3D objects are not affected by Rhino undo
        }
        else if (e.IsEndUndo || e.IsEndRedo)
        {
            // Clear undo-in-progress flag, but mark that we just finished
            // This tells BeforeSolveInstance to skip and schedule a deferred re-solve
            _undoInProgress = false;
            _justFinishedUndo = true;
        }
    }

    /// <summary>
    /// Appends additional menu items to the component's context menu.
    /// </summary>
    protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
    {
        base.AppendAdditionalComponentMenuItems(menu);
        Menu_AppendSeparator(menu);

        var replaceItem = Menu_AppendItem(
            menu,
            "Replace Previous Object",
            this.OnReplaceMenuClick,
            true,
            _replaceEnabled
        );
        replaceItem.ToolTipText = "When enabled, previously created objects will be deleted before creating new ones.";
    }

    /// <summary>
    /// Handles the click event for the Replace menu item.
    /// </summary>
    private void OnReplaceMenuClick(object? sender, EventArgs e)
    {
        _replaceEnabled = !_replaceEnabled;
        if (!_replaceEnabled)
        {
            _lastCreatedObjectIds.Clear();
        }
        this.ExpireSolution(true);
    }

    /// <summary>
    /// Called before the solve cycle starts. Deletes all previously tracked objects
    /// when Replace mode is enabled. Skips the entire solve cycle if an undo/redo
    /// operation just completed (to prevent AccessViolationException in Civil 3D),
    /// and subscribes to AutoCAD Idle event for deferred re-solve when stable.
    /// </summary>
    protected override void BeforeSolveInstance()
    {
        base.BeforeSolveInstance();
        _deletionPerformedThisCycle = false;
        _skipThisSolve = false;

        // If undo is currently in progress, skip this solve entirely
        if (_undoInProgress)
        {
            _skipThisSolve = true;
            // DON'T clear tracked objects - keep them for deferred solve
            return;
        }

        // If undo just finished, skip this solve and schedule a deferred re-solve
        // Civil 3D's internal state is unstable immediately after undo completes
        if (_justFinishedUndo)
        {
            _justFinishedUndo = false;
            _skipThisSolve = true;
            // DON'T clear tracked objects - keep them for deferred solve

            // Schedule re-solve after Civil 3D stabilizes (100ms delay)
            this.OnPingDocument()?.ScheduleSolution(100, d => this.ExpireSolution(false));
            return;
        }

        // Normal execution - delete tracked objects if replace mode enabled
        if (_replaceEnabled && !_deletionPerformedThisCycle)
        {
            this.DeleteAllTrackedObjects();
            _deletionPerformedThisCycle = true;
        }
    }

    /// <summary>
    /// Returns true if this solve cycle should be skipped (post-undo deferral).
    /// Derived classes must check this at the start of their SolveInstance method
    /// and return early if it returns true.
    /// </summary>
    /// <returns>True if the solve should be skipped; otherwise false.</returns>
    protected bool ShouldSkipSolve() => _skipThisSolve;

    /// <summary>
    /// Deletes all tracked objects in batched transactions (one per document).
    /// Uses GetDocumentForObjectId to ensure we use the correct database for each object.
    /// </summary>
    private void DeleteAllTrackedObjects()
    {
        if (_lastCreatedObjectIds.Count == 0)
            return;

        // Group objects by their document to batch deletions
        var objectsByDocument = new Dictionary<IAutocadDocument, List<ObjectId>>();

        foreach (var objectIdWrapper in _lastCreatedObjectIds)
        {
            var objectId = objectIdWrapper.Unwrap();

            // Skip obviously invalid objects
            if (objectId.IsNull || objectId.IsEffectivelyErased)
                continue;

            // Get the correct document for this ObjectId
            var document = this.GetDocumentForObjectId(objectIdWrapper);
            if (document == null)
                continue;

            var database = document.AutocadDatabase.Unwrap();

            // Re-validate by resolving handle from the correct database
            var handle = objectId.Handle;
            if (!database.TryGetObjectId(handle, out var resolvedId))
                continue;

            if (resolvedId.IsNull || resolvedId.IsEffectivelyErased)
                continue;

            if (resolvedId.Database == null || resolvedId.Database != database)
                continue;

            if (!objectsByDocument.TryGetValue(document, out var list))
            {
                list = new List<ObjectId>();
                objectsByDocument[document] = list;
            }
            list.Add(resolvedId);
        }

        // Clear BEFORE attempting deletion
        _lastCreatedObjectIds.Clear();

        // Delete objects grouped by document
        foreach (var kvp in objectsByDocument)
        {
            var document = kvp.Key;
            var objectIds = kvp.Value;

            if (objectIds.Count == 0)
                continue;

            var transactionManager = document.CreateTransactionManager();
            transactionManager.PerformTask(() =>
            {
                var transaction = transactionManager.Unwrap();
                if (transaction == null)
                    return false;

                foreach (var objectId in objectIds)
                {
                    try
                    {
                        if (objectId.IsNull || objectId.IsEffectivelyErased)
                            continue;

                        var dbObject = transaction.GetObject(objectId, OpenMode.ForWrite, false);
                        if (dbObject != null && !dbObject.IsErased)
                        {
                            dbObject.Erase();
                        }
                    }
                    catch
                    {
                        // Skip objects that can't be erased
                    }
                }
                return true;
            });
        }
    }

    /// <summary>
    /// Tracks a created object for potential future deletion.
    /// Called during SolveInstance for each created object.
    /// </summary>
    protected void TrackCreatedObject(ObjectId objectId, IAutocadDocument document)
    {
        if (objectId.IsNull || objectId.IsEffectivelyErased)
            return;

        _lastCreatedObjectIds.Add(new AutocadObjectIdWrapper(objectId));
    }

    /// <summary>
    /// Clears all tracked objects without deleting them.
    /// </summary>
    protected void ClearTrackedObjects()
    {
        _lastCreatedObjectIds.Clear();
    }

    /// <inheritdoc />
    public override bool Read(GH_IReader reader)
    {
        if (!base.Read(reader))
            return false;

        _replaceEnabled = true;
        reader.TryGetBoolean(ReplaceEnabledKey, ref _replaceEnabled);
        _lastCreatedObjectIds.Clear();  // Don't restore object IDs across sessions

        return true;
    }

    /// <inheritdoc />
    public override bool Write(GH_IWriter writer)
    {
        if (!base.Write(writer))
            return false;

        writer.SetBoolean(ReplaceEnabledKey, _replaceEnabled);
        // Don't serialize object IDs - they're not valid across sessions

        return true;
    }
}
