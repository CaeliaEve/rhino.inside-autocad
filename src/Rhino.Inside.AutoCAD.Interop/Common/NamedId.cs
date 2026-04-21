using Rhino.Inside.AutoCAD.Core.Interfaces;
using CadObjectId = Autodesk.AutoCAD.DatabaseServices.ObjectId;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// A record that combines a display name with an AutoCAD ObjectId reference.
/// </summary>
/// <remarks>
/// Implements <see cref="INamedId"/> to provide a simple, immutable container for
/// named object references in Civil 3D. Used for properties like Site, Style,
/// and DesignCheckSet on alignments and other Civil 3D objects.
/// </remarks>
public record NamedId : INamedId
{
    private readonly AutocadObjectIdWrapper _objectIdWrapper;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public IObjectId ObjectId => _objectIdWrapper;

    /// <inheritdoc />
    public bool IsValid => _objectIdWrapper.IsValid;

    /// <summary>
    /// Gets an empty NamedId representing an invalid or null reference.
    /// </summary>
    public static NamedId Empty { get; } = new(string.Empty, CadObjectId.Null);

    /// <summary>
    /// Initializes a new instance of <see cref="NamedId"/> with the specified name
    /// and AutoCAD ObjectId.
    /// </summary>
    /// <param name="name">The display name for this identifier.</param>
    /// <param name="objectId">The AutoCAD ObjectId to wrap.</param>
    public NamedId(string name, CadObjectId objectId)
    {
        this.Name = name ?? string.Empty;
        _objectIdWrapper = new AutocadObjectIdWrapper(objectId);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="NamedId"/> with the specified name
    /// and ObjectId wrapper.
    /// </summary>
    /// <param name="name">The display name for this identifier.</param>
    /// <param name="objectId">The ObjectId wrapper.</param>
    public NamedId(string name, IObjectId objectId)
    {
        this.Name = name ?? string.Empty;
        _objectIdWrapper = objectId as AutocadObjectIdWrapper
            ?? new AutocadObjectIdWrapper(CadObjectId.Null);
    }

    /// <summary>
    /// Creates a new NamedId with the specified name and AutoCAD ObjectId.
    /// </summary>
    /// <param name="name">The display name.</param>
    /// <param name="objectId">The AutoCAD ObjectId.</param>
    /// <returns>A new <see cref="NamedId"/> instance.</returns>
    public static NamedId Create(string name, CadObjectId objectId)
    {
        return new NamedId(name, objectId);
    }

    /// <summary>
    /// Creates a new NamedId with the specified name and ObjectId wrapper.
    /// </summary>
    /// <param name="name">The display name.</param>
    /// <param name="objectId">The ObjectId wrapper.</param>
    /// <returns>A new <see cref="NamedId"/> instance.</returns>
    public static NamedId Create(string name, IObjectId objectId)
    {
        return new NamedId(name, objectId);
    }

    /// <inheritdoc />
    public INamedId ShallowClone()
    {
        return new NamedId(this.Name, _objectIdWrapper.ShallowClone());
    }

    /// <summary>
    /// Returns a string representation of this NamedId.
    /// </summary>
    /// <returns>A string in the format "Name (ObjectId)" or "Empty NamedId" if invalid.</returns>
    public override string ToString()
    {
        if (!this.IsValid)
            return "Not set";

        return $"{this.Name} ({_objectIdWrapper})";
    }
}
