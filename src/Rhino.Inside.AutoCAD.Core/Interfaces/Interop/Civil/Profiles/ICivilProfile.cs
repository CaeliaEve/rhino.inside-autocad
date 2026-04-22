using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;
/// <summary>
/// A wrapper for Civil3d Profiles
/// </summary>
public interface ICivilProfile
{
    ICivilProfileProperties Properties { get; }

    /// <summary>
    /// Extracts all entities from a Civil 3D Profile as wrapper objects.
    /// </summary>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A list of profile entity wrappers.</returns>
    List<ICivilProfileEntity> GetProfileEntities(IAutocadTransactionManager transactionManager);

    /// <summary>
    /// Returns the profile geometry as a Rhino curve. 
    /// </summary>
    RhinoCurve ExtractCurve(IAutocadTransactionManager transactionManager);

    /// <summary>
    /// Extracts all label groups from a Civil 3D Profile.
    /// </summary>
    List<ICivilProfileLabelGroup> GetProfileLabelGroups(
        IAutocadTransactionManager transactionManager);

    /// <summary>
    /// Tries to get the parent alignment for a profile.
    /// </summary>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>The name of the parent alignment, or empty string if not found.</returns>
    bool TryGetParentAlignmentName(IAutocadTransactionManager transactionManager,
        out ICivilAlignment? alignment);

}
