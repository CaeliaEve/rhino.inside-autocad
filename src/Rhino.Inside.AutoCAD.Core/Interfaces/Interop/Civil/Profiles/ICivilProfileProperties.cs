namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents properties extracted from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// This interface provides access to profile metadata and statistics
/// without requiring direct access to the Civil 3D database object.
/// </remarks>
public interface ICivilProfileProperties
{
    /// <summary>
    /// Gets the name of the profile.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the profile.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the starting point of the profile.
    /// </summary>
    ICivilStationPoint Start { get; }

    /// <summary>
    /// Gets the ending point of the profile.
    /// </summary>
    ICivilStationPoint End { get; }

    /// <summary>
    /// Gets the Minimum point of the profile.
    /// </summary>
    double MinElevation { get; }

    /// <summary>
    /// Gets the Maximum point of the profile.
    /// </summary>
    double MaxElevation { get; }

    /// <summary>
    /// Gets the profile type.
    /// </summary>
    CivilProfileType ProfileType { get; }

    /// <summary>
    /// Gets the Id of the parent alignment containing this profile.
    /// </summary>
    IObjectId ParentAlignmentId { get; }

    /// <summary>
    /// Gets the style applied to this profile as a NamedId.
    /// </summary>
    /// <remarks>
    /// Provides both the style name and ObjectId reference.
    /// </remarks>
    INamedId Style { get; }

    /// <summary>
    /// Gets the Id of the profile in the Civil 3D database.
    /// </summary>
    IObjectId ProfileId { get; }

    /// <summary>
    /// Updates the profile with new properties and returns a new
    /// Profile properties object.
    /// </summary>
    ICivilProfileProperties Update(IAutocadTransactionManager transactionManager,
        string newName, string newDescription);
}
