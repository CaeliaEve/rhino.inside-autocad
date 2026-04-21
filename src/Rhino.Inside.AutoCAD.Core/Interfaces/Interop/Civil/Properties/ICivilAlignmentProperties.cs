namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents properties extracted from a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// This interface provides access to alignment metadata and statistics
/// without requiring direct access to the Civil 3D database object.
/// </remarks>
public interface ICivilAlignmentProperties
{
    /// <summary>
    /// Gets the name of the alignment.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the alignment.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the starting station of the alignment.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the ending station of the alignment.
    /// </summary>
    double EndStation { get; }

    /// <summary>
    /// Gets the total length of the alignment.
    /// </summary>
    double Length { get; }

    /// <summary>
    /// Gets the alignment type as an integer.
    /// </summary>
    AlignmentType AlignmentType { get; }

    /// <summary>
    /// Gets the site containing this alignment as a NamedId.
    /// </summary>
    /// <remarks>
    /// Provides both the site name and ObjectId reference.
    /// </remarks>
    INamedId Site { get; }

    /// <summary>
    /// Gets the style applied to this alignment as a NamedId.
    /// </summary>
    /// <remarks>
    /// Provides both the style name and ObjectId reference.
    /// </remarks>
    INamedId Style { get; }

    /// <summary>
    /// Gets the design check set applied to this alignment as a NamedId.
    /// </summary>
    /// <remarks>
    /// Provides both the design check set name and ObjectId reference.
    /// May be empty if no design check set is assigned.
    /// </remarks>
    INamedId DesignCheckSet { get; }

    /// <summary>
    /// Gets the reference station information for this alignment.
    /// </summary>
    ICivilReferenceStation ReferenceStation { get; }

    /// <summary>
    /// Gets the design speeds information for this alignment.
    /// </summary>
    ICivilDesignSpeeds DesignSpeeds { get; }

    /// <summary>
    /// Gets the connected alignment information for this alignment.
    /// </summary>
    ICivilConnectedAlignmentInfo ConnectedAlignmentInfo { get; }

    /// <summary>
    /// Gets the offset alignment information for this alignment.
    /// </summary>
    ICivilOffsetAlignmentInfo OffsetAlignmentInfo { get; }

    /// <summary>
    /// Gets the rail alignment information for this alignment.
    /// </summary>
    ICivilRailAlignmentInfo RailAlignmentInfo { get; }

    /// <summary>
    /// Updates the alignment with a new properties and returns a new
    /// Alignment properties object.
    /// </summary>
    ICivilAlignmentProperties Update(IAutocadTransactionManager transactionManager,
        string newName, string newDescription);

    /// <summary>
    /// Gets the CANT information for this alignment. This requires an active transaction.
    /// </summary>
    ICivilCantInfo GetCantInfo(IAutocadTransactionManager transactionManager);
}
