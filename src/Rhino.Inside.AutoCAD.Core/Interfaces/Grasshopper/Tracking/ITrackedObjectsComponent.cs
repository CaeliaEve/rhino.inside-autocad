using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Interface for Grasshopper components that track objects they have created in the
/// host Autocad document, enabling the canvas to display tracking state (e.g. a black
/// component capsule and a "Tracking N Objects" message).
/// </summary>
public interface ITrackedObjectsComponent : IGH_Component
{
    /// <summary>
    /// The number of host-document objects currently tracked by this component,
    /// including handles loaded from file that have not yet been resolved.
    /// Implementations must not access the Autocad database - this count is
    /// read at canvas paint time and must be cheap and side-effect free.
    /// </summary>
    int TrackedObjectCount { get; }
}
