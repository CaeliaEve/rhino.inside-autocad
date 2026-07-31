namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// A reference component which can defer document-change expiry by composing an
/// <see cref="IStaleDataTracker"/>. When the tracker is present and auto update is
/// disabled, document changes mark the component stale instead of expiring it.
/// </summary>
/// <seealso cref="IStaleDataTracker"/>
public interface IStaleDataComponent : IReferenceComponent
{
    /// <summary>
    /// Gets the stale data tracker composed by this component, or null when the
    /// component does not support stale tracking and always auto updates.
    /// </summary>
    IStaleDataTracker? StaleTracker { get; }
}
