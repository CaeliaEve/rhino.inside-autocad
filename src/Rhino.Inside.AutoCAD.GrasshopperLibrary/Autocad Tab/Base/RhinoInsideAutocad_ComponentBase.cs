using Autodesk.AutoCAD.DatabaseServices;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// The base class for all Rhino.Inside.AutoCAD Grasshopper components.
/// This class extends the <see cref="GH_Component"/> class and provides mechanisms
/// to update component versions based on changes to the component itself.
/// </summary>
public abstract class RhinoInsideAutocad_ComponentBase : GH_Component
{
    private readonly VersioningIssues _versioningStatus = VersioningIssues.None;

    /// <summary>
    /// The version of the component.
    /// </summary>
    protected IComponentVersion Version { get; private set; }

#if DEBUG || DEBUGNET8
    /// <summary>
    /// Adds versioning information to the instance description in debug builds.
    /// </summary>
    public override string InstanceDescription =>
        $"{base.InstanceDescription}\n{this.GetFullVersionDescription()}";
#endif

    /// <summary>
    /// Overrides the Obsolete property to determine if the component is obsolete
    /// </summary>
    public override bool Obsolete => _versioningStatus.HasFlag(VersioningIssues.Obsolete) || base.Obsolete;

    /// <summary>
    /// Constructs a new instance of the <see cref="RhinoInsideAutocad_ComponentBase"/> class.
    /// </summary>
    protected RhinoInsideAutocad_ComponentBase(
        string name,
        string nickname,
        string description,
        string category,
        string subCategory) : base(name, nickname, description, category, subCategory)
    {
        this.Version = this.GetCurrentVersion();

        ComponentVersionAttribute.TryGetVersionHistory(this.GetType(), out var versionHistory);
        if (this.Obsolete || versionHistory!.IsDeprecated) _versioningStatus |= VersioningIssues.Obsolete;
    }

    /// <summary>
    /// Gets a full version description of the component, including its introduction and deprecation details.
    /// </summary>
    private string GetFullVersionDescription()
    {
        ComponentVersionAttribute.TryGetVersionHistory(this.GetType(), out var versionHistory);

        var versionDescription = string.Empty;

        versionDescription += $"Introduced in v{versionHistory!.Introduced}\n";

        if (this.Obsolete)
        {
            if (versionHistory.TryGetDepreciatedVersion(out var depreciatedVersion))
                versionDescription += $"Obsolete since v{depreciatedVersion}\n";

            foreach (var attribute in this.GetType().GetCustomAttributes(typeof(ObsoleteAttribute), false).Cast<ObsoleteAttribute>())
            {
                if (string.IsNullOrWhiteSpace(attribute.Message) == false)
                    versionDescription += $"{attribute.Message}\n";
            }
        }

        return versionDescription;

    }

    /// <summary>
    /// Gets the current version of the component based on its type and the types of its
    /// input and output parameters.
    /// </summary>
    private IComponentVersion GetCurrentVersion()
    {
        var current = ComponentVersionAttribute.GetCurrentVersion(this.GetType());

        foreach (var input in this.Params.Input)
        {
            var version = ComponentVersionAttribute.GetCurrentVersion(input.GetType());
            if (version > current) current = version;
        }

        foreach (var output in this.Params.Output)
        {
            var version = ComponentVersionAttribute.GetCurrentVersion(output.GetType());
            if (version > current) current = version;
        }

        return new ComponentVersion(current);
    }

    /// <inheritdoc />
    public override bool Read(GH_IReader reader)
    {
        if (!base.Read(reader))
            return false;

        this.Version.Read(reader, this.Name);

        return true;
    }

    /// <inheritdoc />
    public override bool Write(GH_IWriter writer)
    {
        if (!base.Write(writer))
            return false;

        this.Version.Write(writer);

        return true;
    }

    /// <summary>
    /// Returns the provided AutoCAD document or, if null, attempts to retrieve the active document from the AutoCAD application.
    /// </summary>
    protected IAutocadDocument? GetDocumentOrDefault(AutocadDocument? autocadDocument = null)
    {
        return autocadDocument ?? this.GetActiveDocumentFallback();
    }

    /// <summary>
    /// Gets the document that owns the specified ObjectId, falling back to the active document
    /// if the ObjectId is null, invalid, or if no matching document is found.
    /// </summary>
    protected IAutocadDocument? GetDocumentForObjectId(IObjectId? objectId)
    {
        if (objectId is null)
            return this.GetActiveDocumentFallback();

        var nativeObjectId = objectId.Unwrap();

        if (nativeObjectId == ObjectId.Null)
            return this.GetActiveDocumentFallback();

        var database = nativeObjectId.Database;
        if (database is null)
            return this.GetActiveDocumentFallback();

        var autoCadInstance = RhinoInsideAutoCadExtension.Application?.RhinoInsideManager?.AutoCadInstance;
        var document = autoCadInstance?.FindDocumentByFingerprintGuid(database.FingerprintGuid);

        return document ?? this.GetActiveDocumentFallback();
    }

    private IAutocadDocument? GetActiveDocumentFallback()
    {
        return RhinoInsideAutoCadExtension.Application?.RhinoInsideManager?.AutoCadInstance?.ActiveDocument;
    }
}
