using Rhino.ApplicationSettings;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Services;

namespace Rhino.Inside.AutoCAD.Interop;

/// <inheritdoc cref="IRhinoInstance"/>
public class RhinoInstance : IRhinoInstance
{
    private readonly IInstallationDirectories _installationDirectories;
    private const string _defaultTemplate = ApplicationConstants.DefaultTemplateFormat;
    private const string _failedToLoadRhinoDoc = ApplicationConstants.FailedToLoadRhinoDoc;

    /// <inheritdoc />
    public event EventHandler? DocumentCreated;

    /// <inheritdoc />
    public event EventHandler? UnitsChanged;

    /// <inheritdoc />
    public event EventHandler<IRhinoObjectModifiedEventArgs>? ObjectModifiedOrAppended;

    /// <inheritdoc />
    public event EventHandler<IRhinoObjectModifiedEventArgs>? ObjectRemoved;

    /// <inheritdoc />
    public event EventHandler? DeselectAll;

    /// <inheritdoc />
    public IRhinoCoreExtension RhinoCore { get; }

    /// <inheritdoc />
    public RhinoDoc? ActiveDoc { get; private set; }

    /// <inheritdoc />
    public Version ApplicationVersion { get; }

    /// <inheritdoc />
    public UnitSystem UnitSystem { get; private set; }

    /// <summary>
    /// Constructs a new <see cref="RhinoInstance"/> for managing the Rhino Inside lifecycle.
    /// </summary>
    /// <param name="installationDirectories">
    /// The installation directories containing paths to resources such as templates.
    /// </param>
    /// <remarks>
    /// This constructor only initializes the management instance; Rhino is not yet running.
    /// Use <see cref="IRhinoLauncher"/> to start a running Rhino instance, then call
    /// <see cref="ValidateRhinoDoc"/> to create or verify the active document.
    /// </remarks>
    /// <seealso cref="IRhinoLauncher"/>
    /// <seealso cref="ValidateRhinoDoc"/>
    public RhinoInstance(IInstallationDirectories installationDirectories)
    {
        _installationDirectories = installationDirectories;
        this.RhinoCore = RhinoCoreExtension.Instance;
        this.ApplicationVersion = Rhino.RhinoApp.Version;

    }

    /// <summary>
    /// Creates and initializes a new <see cref="RhinoDoc"/> based on the specified mode.
    /// </summary>
    /// <param name="logger">
    /// The startup logger used to record errors if document creation fails.
    /// </param>
    /// <param name="mode">
    /// The mode determining whether to create a headless or interactive document.
    /// </param>
    /// <returns>
    /// The newly created <see cref="RhinoDoc"/> instance.
    /// </returns>
    /// <remarks>
    /// This method performs the following initialization steps:
    /// <list type="bullet">
    ///   <item>Creates a headless or interactive document based on <paramref name="mode"/></item>
    ///   <item>Disables auto-save to prevent unwanted file operations</item>
    ///   <item>Raises the <see cref="DocumentCreated"/> event</item>
    ///   <item>Subscribes to Rhino document events for object tracking</item>
    /// </list>
    /// </remarks>
    /// <exception cref="Exception">
    /// Thrown when document creation fails. The error is logged before re-throwing.
    /// </exception>
    /// <seealso cref="ValidateRhinoDoc"/>
    private RhinoDoc CreateRhinoDoc(IStartUpLogger logger,
        RhinoInsideMode mode)
    {
        var template = string.Format(_defaultTemplate, _installationDirectories.Resources);

        try
        {

            var rhinoDoc = mode == RhinoInsideMode.Headless
                ? RhinoDoc.CreateHeadless(template)
                : RhinoDoc.Create(template);

            FileSettings.AutoSaveEnabled = false;

            this.DocumentCreated?.Invoke(this, EventArgs.Empty);

            this.UnitSystem = rhinoDoc.ModelUnitSystem;

            RhinoDoc.DocumentPropertiesChanged += this.OnDocumentPropertiesModified;
            RhinoDoc.AddRhinoObject += this.OnAddRhinoObject;
            RhinoDoc.ModifyObjectAttributes += this.OnModifyRhinoObject;
            RhinoDoc.DeleteRhinoObject += this.OnRemoveRhinoObject;
            RhinoDoc.SelectObjects += this.OnSelectedObject;
            RhinoDoc.DeselectObjects += this.OnSelectedObject;
            RhinoDoc.DeselectAllObjects += this.OnDeselectObjects;

            return rhinoDoc;
        }
        catch
        {
            logger.AddError(_failedToLoadRhinoDoc);

            throw;
        }
    }

    /// <summary>
    /// Handles the <see cref="RhinoDoc.DeselectAllObjects"/> event by raising <see cref="DeselectAll"/>.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event arguments containing deselection details.</param>
    private void OnDeselectObjects(object? sender, RhinoDeselectAllObjectsEventArgs e)
    {
        this.DeselectAll?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Handles <see cref="RhinoDoc.SelectObjects"/> and <see cref="RhinoDoc.DeselectObjects"/> events
    /// by raising <see cref="ObjectModifiedOrAppended"/> for each affected object.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event arguments containing the selected or deselected objects.</param>
    /// <remarks>
    /// Rhino does not raise a modify event when an object's selection state changes, so this method
    /// bridges that gap by treating selection changes as modifications. This ensures preview geometry
    /// is updated to reflect the current selection state.
    /// </remarks>
    private void OnSelectedObject(object? sender, RhinoObjectSelectionEventArgs e)
    {
        for (var index = 0; index < e.RhinoObjects.Length; index++)
        {
            var rhinoObject = e.RhinoObjects[index];

            this.ObjectModifiedOrAppended?.Invoke(this,
                new RhinoObjectModifiedEventArgs(rhinoObject));
        }
    }

    /// <summary>
    /// Handles the <see cref="RhinoDoc.DeleteRhinoObject"/> event by raising <see cref="ObjectRemoved"/>.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event arguments containing the removed object.</param>
    private void OnRemoveRhinoObject(object sender, RhinoObjectEventArgs e)
    {
        this.ObjectRemoved?.Invoke(this, new RhinoObjectModifiedEventArgs(e.TheObject));
    }

    /// <summary>
    /// Handles the <see cref="RhinoDoc.ModifyObjectAttributes"/> event by raising <see cref="ObjectModifiedOrAppended"/>.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event arguments containing the modified object.</param>
    private void OnModifyRhinoObject(object sender, RhinoModifyObjectAttributesEventArgs e)
    {
        this.ObjectModifiedOrAppended?.Invoke(this, new RhinoObjectModifiedEventArgs(e.RhinoObject));
    }

    /// <summary>
    /// Handles the <see cref="RhinoDoc.AddRhinoObject"/> event by raising <see cref="ObjectModifiedOrAppended"/>.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event arguments containing the added object.</param>
    private void OnAddRhinoObject(object sender, RhinoObjectEventArgs e)
    {
        this.ObjectModifiedOrAppended?.Invoke(this, new RhinoObjectModifiedEventArgs(e.TheObject));
    }

    /// <summary>
    /// Handles the <see cref="RhinoDoc.DocumentPropertiesChanged"/> event and raises <see cref="UnitsChanged"/>
    /// when the model unit system has changed.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event arguments containing the document.</param>
    /// <remarks>
    /// This method compares the current <see cref="UnitSystem"/> with the document's model unit system.
    /// If they differ, it raises the <see cref="UnitsChanged"/> event and updates the cached value.
    /// </remarks>
    private void OnDocumentPropertiesModified(object sender, DocumentEventArgs e)
    {
        var currentUnits = e.Document.ModelUnitSystem;

        if (currentUnits == this.UnitSystem)
            return;

        this.UnitsChanged?.Invoke(this, EventArgs.Empty);

        this.UnitSystem = currentUnits;
    }

    /// <inheritdoc />
    public void ValidateRhinoDoc(RhinoInsideMode mode, IStartUpLogger logger)
    {
        if (this.ActiveDoc == null)
        {
            this.ActiveDoc = this.CreateRhinoDoc(logger, mode);
        }
    }

    /// <inheritdoc />
    public Result RunRhinoCommand(string commandName)
    {
        return this.ActiveDoc == null
            ? Result.Failure
            : RhinoApp.ExecuteCommand(this.ActiveDoc, commandName);
    }

    /// <inheritdoc />
    public bool RunRhinoScript(string commandName)
    {
        return this.ActiveDoc != null
               && RhinoApp.RunScript(this.ActiveDoc.RuntimeSerialNumber, commandName, true);
    }

    /// <inheritdoc />
    public void Shutdown()
    {
        RhinoDoc.DocumentPropertiesChanged -= this.OnDocumentPropertiesModified;

        RhinoDoc.AddRhinoObject -= this.OnAddRhinoObject;

        RhinoDoc.ModifyObjectAttributes -= this.OnModifyRhinoObject;

        RhinoDoc.DeleteRhinoObject -= this.OnRemoveRhinoObject;

        RhinoDoc.SelectObjects -= this.OnSelectedObject;

        RhinoDoc.DeselectObjects -= this.OnSelectedObject;

        RhinoDoc.DeselectAllObjects -= this.OnDeselectObjects;

        this.RhinoCore.Shutdown();
    }
}