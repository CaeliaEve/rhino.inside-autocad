namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a baseline extracted from a Civil 3D Corridor.
/// </summary>
/// <remarks>
/// A baseline consists of an alignment and profile pair that defines
/// the horizontal and vertical path of the corridor.
/// </remarks>
public interface ICivilCorridorBaseline
{
    /// <summary>
    /// Gets the name of the baseline.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the ObjectId of the alignment associated with this baseline.
    /// </summary>
    IObjectId AlignmentId { get; }

    /// <summary>
    /// Gets the ObjectId of the profile associated with this baseline.
    /// </summary>
    IObjectId ProfileId { get; }

    /// <summary>
    /// Gets the starting station of the baseline.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the ending station of the baseline.
    /// </summary>
    double EndStation { get; }

    /// <summary>
    /// Extracts all regions from a Civil 3D Corridor Baseline as wrapper objects.
    /// </summary>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A list of baseline region wrappers.</returns>
    List<ICivilCorridorBaselineRegion> GetRegions(
        IAutocadTransactionManager transactionManager);

    /// <summary>
    /// Extracts all feature lines from a Civil 3D Corridor Baseline as wrapper objects.
    /// </summary>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A list of feature line wrappers.</returns>
    List<ICivilCorridorFeatureLine> GetFeatureLines(
        IAutocadTransactionManager transactionManager);
}
