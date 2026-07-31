namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Defines the keys used to persist Grasshopper component and goo state to the
/// Grasshopper document archive.
/// </summary>
/// <remarks>
/// Every value in this class is written into saved <c>.gh</c> files. Changing one
/// breaks deserialization of documents saved by earlier builds: the reader falls back
/// to its default and the user's setting or connection is silently lost. Add new keys
/// rather than renaming existing ones.
/// </remarks>
public class GrasshopperKeys
{
    /// <summary>
    /// Key for the Auto Update toggle, shared by every component that supports stale
    /// data tracking.
    /// </summary>
    /// <remarks>
    /// Read back as false when absent, so documents saved before the feature existed
    /// keep stale tracking as their default behaviour.
    /// </remarks>
    public const string AutoUpdateEnabled = "AutoUpdateEnabled";

    /// <summary>
    /// Key for the Driven Button toggle, which controls whether the Bake button is
    /// shown on the bake component.
    /// </summary>
    public const string DrivenButtonEnabled = "DrivenButtonEnabled";

    /// <summary>
    /// Key for the Replace Previous Object toggle on creation components.
    /// </summary>
    /// <remarks>Read back as true when absent.</remarks>
    public const string ReplaceEnabled = "ReplaceEnabled";

    /// <summary>
    /// Key for the Save Connection Between Sessions toggle on creation components.
    /// </summary>
    /// <remarks>Read back as true when absent.</remarks>
    public const string SaveConnectionEnabled = "SaveConnectionEnabled";

    /// <summary>
    /// Key for the comma separated handles of the objects a creation component tracks
    /// in the AutoCAD document.
    /// </summary>
    public const string TrackedObjectHandles = "TrackedObjectHandles";

    /// <summary>
    /// Key for the signature of the inputs that produced a creation component's
    /// currently tracked objects, used to detect whether a re-solve needs to recreate them.
    /// </summary>
    public const string LastInputSignature = "LastInputSignature";

    /// <summary>
    /// Key for the AutoCAD reference handle stored on a referenced goo type.
    /// </summary>
    /// <remarks>
    /// Shared by every referencing goo in both the AutoCAD and Civil Grasshopper
    /// libraries, so a single value keeps their read and write paths in step.
    /// </remarks>
    public const string AutocadReferenceHandle = "AutocadReferenceHandle";
}
