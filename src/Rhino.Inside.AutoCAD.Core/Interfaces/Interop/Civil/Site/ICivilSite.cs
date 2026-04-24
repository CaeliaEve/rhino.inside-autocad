namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a Civil 3D Site container.
/// </summary>
/// <remarks>
/// Sites are containers in Civil 3D that hold parcels, alignments,
/// grading groups, and other site-related objects. This interface
/// provides access to site properties and collections.
/// </remarks>
public interface ICivilSite : INamedDbObject
{
    /// <summary>
    /// Gets the description of the site.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the number of parcels in the site.
    /// </summary>
    int ParcelCount { get; }

    /// <summary>
    /// Gets the number of alignments in the site.
    /// </summary>
    int AlignmentCount { get; }

    /// <summary>
    /// Gets the parcel ObjectIds in the site.
    /// </summary>
    IReadOnlyList<IObjectId> ParcelIds { get; }

    /// <summary>
    /// Gets the alignment ObjectIds in the site.
    /// </summary>
    IReadOnlyList<IObjectId> AlignmentIds { get; }

    /// <summary>
    /// Updates the site with new properties and returns a new
    /// Site wrapper object.
    /// </summary>
    ICivilSite Update(IAutocadTransactionManager transactionManager,
        string newName, string newDescription);
}
