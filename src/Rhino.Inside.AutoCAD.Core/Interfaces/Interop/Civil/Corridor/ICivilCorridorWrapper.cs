namespace Rhino.Inside.AutoCAD.Core.Interfaces;

public interface ICivilCorridorWrapper
{
    ICivilCorridorProperties Properties { get; }

    /// <summary>
    /// Extracts all baselines from a Civil 3D Corridor as wrapper objects.
    /// </summary>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A list of baseline wrappers.</returns>
    List<ICivilCorridorBaseline> GetBaselines(IAutocadTransactionManager transactionManager);

    /// <summary>
    /// Extracts all corridor surfaces from a Civil 3D Corridor as wrapper objects.
    /// </summary>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A list of corridor surface wrappers.</returns>
    List<ICivilCorridorSurface> GetCorridorSurfaces(IAutocadTransactionManager transactionManager);
}