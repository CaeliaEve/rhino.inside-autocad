using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents properties extracted from a Civil 3D ProfileView.
/// </summary>
/// <remarks>
/// This interface provides access to ProfileView metadata and display settings
/// without requiring direct access to the Civil 3D database object.
/// </remarks>
public interface ICivilProfileViewProperties
{
    /// <summary>
    /// Gets the style applied to this profile view as a NamedId.
    /// </summary>
    /// <remarks>
    /// Provides both the style name and ObjectId reference.
    /// </remarks>
    INamedId Style { get; }

    /// <summary>
    /// Gets the station range (start to end) of the ProfileView display.
    /// </summary>
    Interval StationRange { get; }

    /// <summary>
    /// Gets the elevation range (min to max) of the ProfileView display.
    /// </summary>
    Interval ElevationRange { get; }

    /// <summary>
    /// The Id of the ProfileView in the Civil 3D database, used for reference and potential future operations.
    /// </summary>
    IObjectId ProfileViewId { get; }

    /// <summary>
    /// Gets the name of the ProfileView.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the ProfileView.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets The coordinate system of the ProfileView
    /// </summary>
    IProfileViewCoordinateSystem GetCoordinateSystem(
        IAutocadTransactionManager transactionManager);

    /// <summary>
    /// Updates the profile view with new properties and returns a new
    /// ProfileView properties object.
    /// </summary>
    ICivilProfileViewProperties Update(IAutocadTransactionManager transactionManager,
        string newName, string newDescription);
}