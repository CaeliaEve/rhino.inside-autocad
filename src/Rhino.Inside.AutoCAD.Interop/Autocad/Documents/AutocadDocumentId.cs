using Rhino.Inside.AutoCAD.Core.Interfaces;
using System.Runtime.CompilerServices;
using Document = Autodesk.AutoCAD.ApplicationServices.Document;

namespace Rhino.Inside.AutoCAD.Interop;

/// <inheritdoc cref="IAutocadDocumentId"/>
public class AutocadDocumentId : IAutocadDocumentId
{
    /// <summary>
    /// Runtime identifiers, keyed weakly so that a document is not held alive by this table.
    /// </summary>
    private static readonly ConditionalWeakTable<Document, object> _runtimeIds = new();

    /// <inheritdoc/>
    public Guid DrawingId { get; }

    /// <inheritdoc/>
    public Guid RuntimeId { get; }

    /// <summary>
    /// Constructs a new <see cref="IAutocadDocumentId"/>.
    /// </summary>
    /// <param name="document">
    /// The document to identify.
    /// </param>
    public AutocadDocumentId(IAutocadDocument document)
    {
        var nativeDocument = document.Unwrap();

        var fingerprint = nativeDocument.Database.FingerprintGuid;

        this.RuntimeId = GetRuntimeId(nativeDocument);

        this.DrawingId = Guid.TryParse(fingerprint, out var drawingId)
            ? drawingId
            : this.RuntimeId;
    }

    /// <summary>
    /// Returns the runtime identifier of an open AutoCAD document, minting one on first use.
    /// </summary>
    /// <param name="document">
    /// The native document to identify.
    /// </param>
    /// <remarks>
    /// Cached against the native document rather than assigned per wrapper, so that the
    /// identifier can be obtained from a bare <see cref="Document"/> before any wrapper
    /// exists. That is what allows callers to ask whether a document is already tracked
    /// without constructing a wrapper in order to find out.
    /// </remarks>
    public static Guid GetRuntimeId(Document document)
    {
        var runtimeId = _runtimeIds.GetValue(document, _ => Guid.NewGuid());

        return (Guid)runtimeId;
    }
}
