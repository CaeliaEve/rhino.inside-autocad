namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a named identifier that combines a display name with an AutoCAD ObjectId.
/// </summary>
/// <remarks>
/// This interface provides a convenient way to pass around references to named Civil 3D
/// objects (such as Sites, Styles, or Parent Alignments) that have both a human-readable
/// name and a database reference. Used in alignment properties for Site, Style, and
/// DesignCheckSet references.
/// </remarks>
public interface INamedId
{
    /// <summary>
    /// Gets the display name associated with this identifier.
    /// </summary>
    /// <remarks>
    /// This is typically the name property of the referenced Civil 3D object.
    /// May be empty if the object has no name or if the reference is invalid.
    /// </remarks>
    string Name { get; }

    /// <summary>
    /// Gets the ObjectId reference to the underlying database object.
    /// </summary>
    /// <remarks>
    /// Provides direct access to the Civil 3D object in the database.
    /// The ObjectId may be null or invalid if the referenced object no longer exists.
    /// </remarks>
    IObjectId ObjectId { get; }

    /// <summary>
    /// Gets a value indicating whether this NamedId represents a valid reference.
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> if the ObjectId is valid and references an existing object;
    /// otherwise <c>false</c>.
    /// </remarks>
    bool IsValid { get; }

    /// <summary>
    /// Creates a shallow copy of this NamedId.
    /// </summary>
    /// <returns>
    /// A new <see cref="INamedId"/> instance with the same name and ObjectId reference.
    /// </returns>
    INamedId ShallowClone();
}
