namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a unique identifier for an AutoCAD document, taken from the fingerprint that
/// AutoCAD itself assigns to a drawing and stores in its header. The identifier is
/// therefore consistent across sessions and can be used to track the document, without the
/// application having to write anything to the drawing to obtain it.
/// </summary>
/// <remarks>
/// Reading the identifier must never modify the document. Deriving it from a value AutoCAD
/// already maintains keeps activating a drawing free of side effects, which matters because
/// documents are wrapped from native reactor callbacks, where a failed write terminates the
/// host, and because drawings can be opened read-only.
/// </remarks>
public interface IAutocadDocumentId
{
    /// <summary>
    /// The unique identifier for the drawing this document was opened from.
    /// </summary>
    /// <remarks>
    /// Identifies the <em>drawing</em>, not the open document, so two copies of the same
    /// file opened at once share it — the fingerprint is copied along with the file. Use
    /// <see cref="RuntimeId"/> to tell open documents apart. Falls back to
    /// <see cref="RuntimeId"/> when the document's database reports no parsable fingerprint.
    /// </remarks>
    Guid DrawingId { get; }

    /// <summary>
    /// The unique identifier for this open document within the current AutoCAD session.
    /// </summary>
    /// <remarks>
    /// Never shared between two open documents, including two copies of the same drawing,
    /// which makes it the correct key for tracking open documents. It is minted per open
    /// document and does not survive a restart, so it must not be persisted or compared
    /// across sessions — <see cref="DrawingId"/> is the durable identity.
    /// </remarks>
    Guid RuntimeId { get; }
}
