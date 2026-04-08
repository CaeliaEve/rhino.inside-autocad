namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Models the host Autocad Application.
/// The application is attached to this object and persists for its lifetime.
/// </summary>
public interface IAutoCadInstance
{
    /// <summary>
    /// Event raised when the Autocad document changes, e.g. a new document is opened.
    /// </summary>
    event EventHandler? DocumentCreated;

    /// <summary>
    /// Event raised when the units of the Autocad document change.
    /// </summary>
    event EventHandler? UnitsChanged;

    /// <summary>
    /// Event raised when the Autocad document is modified, eg. objects are added or removed.
    /// </summary>
    event EventHandler<IAutocadDocumentChangeEventArgs>? DocumentChanged;

    /// <summary>
    /// The <see cref="IStartUpLogger"/> for this <see cref="IAutoCadInstance"/>.
    /// </summary>
    IStartUpLogger StartUpLogger { get; }

    /// <summary>
    /// The list of open <see cref="IAutocadDocument"/>s in the AutoCAD application.
    /// </summary>
    List<IAutocadDocument> Documents { get; }

    /// <summary>
    /// The current active <see cref="IAutocadDocument"/> in the AutoCAD application.
    /// </summary>
    IAutocadDocument? ActiveDocument { get; }

    /// <summary>
    /// The version of the AutoCAD application.
    /// </summary>
    Version ApplicationVersion { get; }

    /// <summary>
    /// A boolean indicating whether the current AutoCAD profile is a Civil 3D profile.
    /// This is determined by checking the registry key for the current profile.
    /// If the application is not "IsPureAcadProfile" = "0" then it is assumed to be a
    /// Civil 3D profile, and this property will return true.
    /// </summary>
    bool IsCivil3d { get; }

    /// <summary>
    /// Ensures that the AutoCAD instance is properly shutdown and resources are released.
    /// </summary>
    void Shutdown();
}