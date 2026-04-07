using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Carries the list of objects whose selection state just changed.
/// </summary>
public interface IGrasshopperSelectionEventArgs
{
    /// <summary>
    /// The objects that were selected or deselected.
    /// </summary>
    IReadOnlyList<IGH_DocumentObject> Objects { get; }
}