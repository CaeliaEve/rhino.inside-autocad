namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Tracks whether the data output by a component still reflects the AutoCAD document.
/// When auto update is disabled, document changes are accumulated and the component is
/// marked stale instead of being expired, until the user refreshes it.
/// </summary>
/// <seealso cref="IStaleDataComponent"/>
public interface IStaleDataTracker
{
    /// <summary>
    /// Gets a value indicating whether auto update is enabled. When enabled, document
    /// changes expire the component immediately (via <see cref="IReferenceComponent.NeedsToBeExpired"/>)
    /// and no stale tracking occurs. When disabled, changes mark the component stale instead.
    /// </summary>
    bool AutoUpdateEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether the component's outputs no longer reflect
    /// the AutoCAD document.
    /// </summary>
    bool IsStale { get; }

    /// <summary>
    /// Called by the change responder when <see cref="AutoUpdateEnabled"/> is false.
    /// Accumulates stale change counts and updates the component display.
    /// </summary>
    /// <param name="change">The document change to process.</param>
    /// <returns>
    /// True if the component must still be expired (for example a wired reference
    /// param was affected by the change); otherwise false.
    /// </returns>
    bool NotifyDocumentChanged(IAutocadDocumentChange change);

    /// <summary>
    /// Clears the stale state and re-solves the component.
    /// </summary>
    void Refresh();
}
