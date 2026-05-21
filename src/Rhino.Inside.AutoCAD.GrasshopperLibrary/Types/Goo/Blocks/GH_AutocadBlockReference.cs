using Autodesk.AutoCAD.DatabaseServices;
using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for AutoCAD block instances.
/// </summary>
public class GH_AutocadBlockReference : GH_AutocadObjectGoo<AutocadBlockReferenceWrapper>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_AutocadBlockReference"/> class with no value.
    /// </summary>
    public GH_AutocadBlockReference()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_AutocadBlockReference"/> class with the
    /// specified AutoCAD block instance.
    /// </summary>
    /// <param name="autocadBlockRefWrapper">The AutoCAD block instance to wrap.</param>
    public GH_AutocadBlockReference(AutocadBlockReferenceWrapper autocadBlockRefWrapper) : base(autocadBlockRefWrapper)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_AutocadBlockReference"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_AutocadBlockReference(GH_AutocadBlockReference other)
    {
        this.Value = other.Value;
    }

    /// <summary>
    /// Constructs a new <see cref="GH_AutocadBlockReference"/> via the interface.
    /// </summary>
    public GH_AutocadBlockReference(IAutocadBlockReference autocadBlockReference)
        : base((autocadBlockReference as AutocadBlockReferenceWrapper)!)
    {
    }

    /// <inheritdoc />
    protected override Type GetCadType() => typeof(BlockReference);

    /// <inheritdoc />
    protected override IGH_Goo CreateInstance(IDbObject dbObject)
    {
        var unwrapped = dbObject.UnwrapObject();

        var newWrapper = new AutocadBlockReferenceWrapper(unwrapped as BlockReference);

        return new GH_AutocadBlockReference(newWrapper);
    }
}