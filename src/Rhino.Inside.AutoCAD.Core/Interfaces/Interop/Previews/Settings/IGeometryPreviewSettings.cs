namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Provides settings for previewing geometry in AutoCAD.
/// </summary>
public interface IGeometryPreviewSettings
{
    /// <summary>
    /// Gets or sets the AutoCAD Color Index used for the preview geometry.
    /// </summary>
    /// <remarks>
    /// Setting this only changes the previews drawn from here on. Existing previews are
    /// restyled by <see cref="IPreviewServer.RefreshAppearance"/>, and the material named by
    /// <see cref="MaterialName"/> has to be recreated with <see cref="CreateMaterial"/>.
    /// </remarks>
    int ColorIndex { get; set; }

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
}