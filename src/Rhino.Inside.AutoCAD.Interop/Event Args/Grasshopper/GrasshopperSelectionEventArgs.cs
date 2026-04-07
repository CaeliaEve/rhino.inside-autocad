using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Interop;

/// <inheritdoc cref="IGrasshopperSelectionEventArgs"/>
public class GrasshopperSelectionEventArgs : EventArgs, IGrasshopperSelectionEventArgs
{
    /// <inheritdoc />
    public IReadOnlyList<IGH_DocumentObject> Objects { get; }

    /// <summary>
    /// Constructs a new instance of <see cref="IGrasshopperSelectionEventArgs"/> with the given objects.
    /// </summary>
    public GrasshopperSelectionEventArgs(IEnumerable<IGH_DocumentObject> objects)
    {
        this.Objects = objects?.ToList().AsReadOnly()
                       ?? throw new ArgumentNullException(nameof(objects));
    }
}