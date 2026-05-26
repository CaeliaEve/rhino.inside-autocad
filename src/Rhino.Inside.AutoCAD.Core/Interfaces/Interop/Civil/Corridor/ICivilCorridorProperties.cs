namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents properties extracted from a Civil 3D Corridor.
/// </summary>
/// <remarks>
/// This interface provides access to corridor metadata and statistics
/// without requiring direct access to the Civil 3D database object.
/// </remarks>
public interface ICivilCorridorProperties
{
    /// <summary>
    /// Gets the name of the corridor.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the corridor.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the code of the corridor as a NamedId.
    /// </summary>
    /// <remarks>
    /// Provides both the code name and ObjectId reference.
    /// </remarks>
    INamedId Code { get; }

    /// <summary>
    /// Gets the starting parameter of the corridor.
    /// </summary>
    double StartParam { get; }

    /// <summary>
    /// Gets the ending parameter of the corridor.
    /// </summary>
    double EndParam { get; }

    /// <summary>
    /// Gets the style applied to this corridor as a NamedId.
    /// </summary>
    /// <remarks>
    /// Provides both the style name and ObjectId reference.
    /// </remarks>
    INamedId Style { get; }

    /// <summary>
    /// Gets the object id of the corridor.
    /// </summary>
    IObjectId CorridorId { get; }

    /// <summary>
    /// Updates the corridor with new properties and returns a new
    /// Corridor properties object.
    /// </summary>
    /// <param name="transactionManager">The transaction manager to use for the update.</param>
    /// <param name="newName">The new name for the corridor.</param>
    /// <param name="newDescription">The new description for the corridor.</param>
    /// <param name="newCode">The new code set style name for the corridor.</param>
    /// <returns>A new <see cref="ICivilCorridorProperties"/> instance with updated values.</returns>
    ICivilCorridorProperties Update(IAutocadTransactionManager transactionManager,
        string newName, string newDescription, string newCode);
}
