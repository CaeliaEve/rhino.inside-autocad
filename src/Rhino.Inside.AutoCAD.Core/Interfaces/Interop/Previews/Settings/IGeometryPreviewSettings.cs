namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Provides settings for previewing geometry in AutoCAD.
/// </summary>
public interface IGeometryPreviewSettings
{
    /// <summary>
    /// Gets the color index used for the preview geometry.
    /// </summary>
    int ColorIndex { get; }

    /// <summary>
    /// Gets the transparency level used for the preview geometry.
    /// </summary>
    byte Transparency { get; }

    /// <summary>
    /// Gets the material ID used for the preview geometry.
    /// </summary>
    IObjectId MaterialId { get; }

    /// <summary>
    /// The name of the material to use.
    /// </summary>
    string MaterialName { get; }

    /// <summary>
    /// Creates the preview material in the AutoCAD database if it does not already exist.
    /// </summary>
    void CreateMaterial(IAutocadDocument document);

    /// <summary>
    /// Ensures <see cref="MaterialId"/> references a live material in the given document's
    /// database, recreating the material if the cached id is stale.
    /// </summary>
    /// <remarks>
    /// The cached id goes stale when the material's document is closed, the material
    /// creation is undone, or a PURGE erases it (transient entities do not count as
    /// database references). It is also stale when it belongs to a different document
    /// than the one previewed into.
    /// </remarks>
    void EnsureMaterial(IAutocadDocument document);

    /// <summary>
    /// Applies these preview settings to the given entity.
    /// </summary>
    void ApplyTo(IEntity entity);
}