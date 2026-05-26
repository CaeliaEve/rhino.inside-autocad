namespace Rhino.Inside.AutoCAD.Core.Interfaces;

public interface ICivilProfileViewWrapper
{
    /// <summary>
    /// The name of the profile View.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The properties of the ProfileView, extracted at construction time.
    /// </summary>
    ICivilProfileViewProperties Properties { get; }

    /// <summary>
    /// Gets all profile displayed in this ProfileView.
    /// </summary>
    List<ICivilProfile> GetDisplayedProfiles(IAutocadTransactionManager transactionManager);

    /// <summary>
    /// Returns the alignment displayed in this ProfileView, if any.
    /// </summary>
    bool TryGetAlignment(IAutocadTransactionManager transactionManager,
        out ICivilAlignment? alignmentWrapped);

    /// <summary>
    /// Extracts band information from a ProfileView.
    /// </summary>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A list of band wrappers.</returns>
    List<ICivilProfileViewBand> GetBands(IAutocadTransactionManager transactionManager);

    /// <summary>
    /// Extracts label group information from a ProfileView.
    /// </summary>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A list of label group wrappers.</returns>
    List<ICivilFeatureLabel> GetProfileViewLabelGroups(IAutocadTransactionManager transactionManager);
}