using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Services;
using System.Windows.Threading;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Rhino.Inside.AutoCAD.Interop;

/// <inheritdoc cref="IAutoCadInstance"/>
public class AutoCadInstance : IAutoCadInstance
{
    private readonly Dispatcher _dispatcher;

    private readonly DocumentCollection? _documentManager;

    private readonly string _readOnlyNotSupported = MessageConstants.ReadOnlyNotSupported;
    private readonly string _fileUnitsNotSupported = MessageConstants.FileUnitsNotSupported;
    private const string _userRegistryProductRootKeyProfiles = MessageConstants.UserRegistryProductRootKeyProfiles;
    private const string _isPureAcadProfile = MessageConstants.IsPureAcadProfile;

    /// <inheritdoc/>
    public event EventHandler? DocumentActivated;

    /// <inheritdoc/>
    public event EventHandler? UnitsChanged;

    /// <inheritdoc/>
    public event EventHandler<IAutocadDocumentChangeEventArgs>? DocumentChanged;

    /// <inheritdoc/>
    public IStartUpLogger StartUpLogger { get; }

    /// <inheritdoc/>
    public List<IAutocadDocument> Documents { get; }

    /// <inheritdoc/>
    public IAutocadDocument? ActiveDocument => this.GetActiveDocument();

    /// <inheritdoc/>
    public Version ApplicationVersion { get; }

    /// <inheritdoc/>
    public bool IsCivil3d { get; }

    /// <summary>
    /// Constructs a new <see cref="IAutoCadInstance"/>.
    /// </summary>
    public AutoCadInstance(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;

        _documentManager = Application.DocumentManager;

        var documentFiles = new List<IAutocadDocument>();
        foreach (var documentObject in _documentManager)
        {
            if (documentObject is Document document == false) continue;

            var documentFile = new AutocadDocument(document, _dispatcher);

            this.SubscribeToDocumentEvents(documentFile);

            documentFiles.Add(documentFile);
        }

        _documentManager.DocumentActivated += this.OnDocumentActivated;

        this.Documents = documentFiles;

        this.StartUpLogger = new StartUpLogger();

        this.ApplicationVersion = Application.Version;

        this.Validate(documentFiles);

        this.IsCivil3d = this.CheckIsCivil();
    }

    /// <summary>
    /// Checks if the current AutoCAD profile is a Civil 3D profile by checking the registry
    /// key for the current profile. If the application is not "IsPureAcadProfile" = "0",
    /// then we assume it is a Civil 3D profile. Not this is not robust if it turns out that
    /// there are other profiles which are not pure AutoCAD profiles.
    /// </summary>
    private bool CheckIsCivil()
    {
        var productKey = HostApplicationServices.Current.UserRegistryProductRootKey;
        productKey += _userRegistryProductRootKeyProfiles;

        var key = Microsoft.Win32.Registry.CurrentUser;
        key = key.OpenSubKey(productKey, false);

        var currentProfile = key.GetValue("").ToString();
        key = key.OpenSubKey(currentProfile, false);

        var keyNames = key.GetValueNames();

        foreach (var valueName in keyNames)
        {
            if (valueName != _isPureAcadProfile) continue;

            if (key.GetValue(valueName).ToString() == "1") return false;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.StartsWith("AeccUiWindows"))
                {
                    return true;
                }
            }
        }

        return false;

    }

    /// <summary>
    /// Subscribes to the relevant document events.
    /// </summary>
    private void SubscribeToDocumentEvents(IAutocadDocument autocadDocument)
    {
        var document = autocadDocument.Unwrap();
        document.BeginDocumentClose += this.OnDocumentClosing;
        autocadDocument.DocumentChanged += this.OnDocumentChanged;
    }

    /// <summary>
    /// Unsubscribes to the relevant document events.
    /// </summary>
    private void UnsubscribeToDocumentEvents(IAutocadDocument autocadDocument)
    {
        var document = autocadDocument.Unwrap();
        document.BeginDocumentClose -= this.OnDocumentClosing;
        autocadDocument.DocumentChanged -= this.OnDocumentChanged;
    }

    /// <summary>
    /// Internal event handler which bubbles up the document modified event.
    /// </summary>
    private void OnDocumentChanged(object sender, IAutocadDocumentChangeEventArgs e)
    {
        if (e.Change.Contains(ChangeType.UnitsChanged))
        {
            this.UnitsChanged?.Invoke(this, EventArgs.Empty);

            if (e.Change.Count() == 1)
                return;
        }

        this.DocumentChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Returns the active document in the AutoCAD application.
    /// </summary>
    private IAutocadDocument? GetActiveDocument()
    {
        foreach (var autoCadDocument in this.Documents)
        {
            if (autoCadDocument.Unwrap().IsActive)
            {
                return autoCadDocument;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public IAutocadDocument? FindDocumentByFingerprintGuid(string fingerprintGuid)
    {
        foreach (var autoCadDocument in this.Documents)
        {
            var nativeDocument = autoCadDocument.Unwrap();
            if (nativeDocument.Database.FingerprintGuid == fingerprintGuid)
            {
                return autoCadDocument;
            }
        }
        return null;
    }

    /// <summary>
    /// Event handler which fires when the <see cref=" DocumentCollection.DocumentActivated"/>
    /// is raised. Raises the <see cref="DocumentActivated"/> event.
    /// </summary>
    protected void OnDocumentActivated(object sender, DocumentCollectionEventArgs e)
    {
        var document = e.Document;

        if (document != null && this.Documents.Any(d => d.Unwrap().Name == document.Name) == false)
        {
            var documentFile = new AutocadDocument(document, _dispatcher);

            document.BeginDocumentClose += this.OnDocumentClosing;

            this.Documents.Add(documentFile);

            this.SubscribeToDocumentEvents(documentFile);
        }

        this.DocumentActivated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Event handler which fires when the <see cref="Document.BeginDocumentClose"/>
    /// event is raised.
    /// </summary>
    protected void OnDocumentClosing(object sender, DocumentBeginCloseEventArgs e)
    {
        var document = sender as Document;

        var autoCadDocument = this.Documents.FirstOrDefault(d => d.Unwrap().Name == document.Name);

        if (autoCadDocument != null)
        {
            document!.BeginDocumentClose -= this.OnDocumentClosing;
            this.Documents.Remove(autoCadDocument);
        }
    }

    /// <summary>
    /// Validates this service by posting any known invalid states to the
    /// <see cref="StartUpLogger"/>.
    /// </summary>
    private void Validate(List<IAutocadDocument> autoCadDocuments)
    {
        foreach (var autoCadDocument in autoCadDocuments)
        {
            var validationLogger = this.StartUpLogger;

            var cadDocument = autoCadDocument.Unwrap();

            if (cadDocument.IsReadOnly)
            {
                validationLogger.AddError(_readOnlyNotSupported);
            }

            var unitSystem = autoCadDocument.UnitSystem;
            if (unitSystem == UnitSystem.Unset)
            {
                validationLogger.AddError(string.Format(_fileUnitsNotSupported,
                    unitSystem));

            }
        }
    }

    /// <inheritdoc/>
    public void Shutdown()
    {
        System.Diagnostics.Debug.WriteLine("=== AutoCadInstance.Shutdown() START ===");
        try
        {
            _documentManager!.DocumentActivated -= this.OnDocumentActivated;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to unsubscribe DocumentActivated: {ex.Message}");
        }

        foreach (var autoCadDocument in this.Documents.ToList())
        {
            try
            {
                this.UnsubscribeToDocumentEvents(autoCadDocument);
                autoCadDocument.CloseDocument();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to close document: {ex.Message}");
            }
        }

        System.Diagnostics.Debug.WriteLine("=== AutoCadInstance.Shutdown() END ===");
    }
}