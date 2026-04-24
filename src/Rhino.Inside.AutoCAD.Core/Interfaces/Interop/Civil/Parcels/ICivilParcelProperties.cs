namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents properties extracted from a Civil 3D Parcel.
/// </summary>
/// <remarks>
/// This interface provides access to parcel metadata and statistics
/// without requiring direct access to the Civil 3D database object.
/// </remarks>
public interface ICivilParcelProperties
{
    /// <summary>
    /// Gets the name of the parcel.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the parcel.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the area of the parcel in square units.
    /// </summary>
    double Area { get; }

    /// <summary>
    /// Gets the perimeter of the parcel.
    /// </summary>
    double Perimeter { get; }

    /// <summary>
    /// Gets the parcel number.
    /// </summary>
    int Number { get; }

    /// <summary>
    /// Gets the tax ID of the parcel.
    /// </summary>
    int TaxId { get; }

    /// <summary>
    /// Gets the address of the parcel.
    /// </summary>
    string Address { get; }

    /// <summary>
    /// Gets the name of the site containing this parcel.
    /// </summary>
    string SiteName { get; }

    /// <summary>
    /// Gets the number of boundary segments in the parcel.
    /// </summary>
    int SegmentCount { get; }

    /// <summary>
    /// Gets whether the parcel boundary is closed.
    /// </summary>
    bool IsClosed { get; }

    /// <summary>
    /// Gets the style applied to this parcel as a NamedId.
    /// </summary>
    /// <remarks>
    /// Provides both the style name and ObjectId reference.
    /// </remarks>
    INamedId Style { get; }

    /// <summary>
    /// Gets the Id of the parcel in the Civil 3D database.
    /// </summary>
    IObjectId ParcelId { get; }

    /// <summary>
    /// Updates the parcel with new properties and returns a new
    /// Parcel properties object.
    /// </summary>
    ICivilParcelProperties Update(IAutocadTransactionManager transactionManager,
        string newName, string newDescription);
}
