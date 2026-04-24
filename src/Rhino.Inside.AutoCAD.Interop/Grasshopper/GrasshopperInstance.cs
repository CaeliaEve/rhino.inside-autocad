using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using System.Reflection;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Represents an implementation of <see cref="IGrasshopperInstance"/> that manages the
/// lifecycle and interactions with Grasshopper within the Rhino.Inside.AutoCAD
/// environment.
/// </summary>
public class GrasshopperInstance : IGrasshopperInstance
{
    private readonly IInstallationDirectories _installationDirectories;

    private readonly bool _loadCivil;
    private const string _grasshopperLibraryFileName = InteropConstants.GrasshopperLibraryFileName;
    private const string _grasshopperCivilLibraryFileName = InteropConstants.GrasshopperCivilLibraryFileName;
    private const string _loadGhaMethodNotFound = MessageConstants.LoadGhaMethodNotFound;
    private const string _grasshopperInitializationFailed = MessageConstants.GrasshopperInitializationFailed;

    private IGrasshopperSelectionTracker? _selectionTracker;
    private GH_Canvas? _activeCanvas;

    /// <inheritdoc />
    public event EventHandler<IGrasshopperObjectModifiedEventArgs>? PreviewExpired;

    /// <inheritdoc />
    public event EventHandler<IGrasshopperObjectModifiedEventArgs>? ObjectRemoved;

    /// <inheritdoc />
    public event EventHandler<IGrasshopperSelectionEventArgs>? ComponentSelectionChanged;

    /// <inheritdoc />
    public GH_Document? ActiveDoc { get; private set; }

    /// <inheritdoc />
    public Version? ApplicationVersion { get; private set; }

    /// <inheritdoc />
    public bool IsEnabled => Grasshopper.Kernel.GH_Document.EnableSolutions;

    /// <summary>
    /// Initializes a new instance of the <see cref="GrasshopperInstance"/> class.
    /// </summary>
    /// <param name="installationDirectories">
    /// The application directories used to locate resources.
    /// </param>
    /// <param name="loadCivil">
    /// A Boolean indicating if the Civil3d grasshopper library will
    /// also be loaded when grasshopper loads
    /// </param>
    public GrasshopperInstance(IInstallationDirectories installationDirectories, bool loadCivil)
    {
        _installationDirectories = installationDirectories;
        _loadCivil = loadCivil;
    }

    /// <summary>
    /// Uses reflection to load the Grasshopper library into the Grasshopper
    /// component server.
    /// </summary>
    /// <exception cref="TargetException">
    /// Thrown if the LoadGHA method is not found.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown if an error occurs while invoking the LoadGHA method.
    /// </exception>
    private void LoadGrasshopperLibrary()
    {
        var assembliesFolder = _installationDirectories.VersionedAssemblies;
        var grasshopperLibraryPath = System.IO.Path.Combine(assembliesFolder, _grasshopperLibraryFileName);
        var grasshopperCivilLibraryPath = System.IO.Path.Combine(assembliesFolder, _grasshopperCivilLibraryFileName);

        var assembly = Assembly.LoadFrom(grasshopperLibraryPath);

        var assemblyInfo = new GH_AssemblyInfoStub(assembly);

        var comparer = new GH_AssemblyInfoStubComparer();

        if (Instances.ComponentServer.Libraries.Contains(assemblyInfo, comparer) ==
            false)
        {
            var loadGhaMethod = typeof(GH_ComponentServer).GetMethod(
                "LoadGHA", BindingFlags.NonPublic | BindingFlags.Instance);

            if (loadGhaMethod == null)
            {
                throw new TargetException(_loadGhaMethodNotFound);
            }

            try
            {
                loadGhaMethod.Invoke(Instances.ComponentServer,
                    [new GH_ExternalFile(grasshopperLibraryPath), false]
                );

                if (_loadCivil)
                {
                    loadGhaMethod.Invoke(Instances.ComponentServer,
                        [new GH_ExternalFile(grasshopperCivilLibraryPath), false]
                    );
                }
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }
    }

    /// <summary>
    /// Loads and initializes the Grasshopper environment.
    /// </summary>
    /// <param name="startUpLogger">
    /// The logger to record validation messages.
    /// </param>
    /// <returns>
    /// The active Grasshopper document.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if Grasshopper fails to initialize.
    /// </exception>
    private void LoadGrasshopper(IStartUpLogger startUpLogger)
    {
        try
        {

            GooTypeRegistry.Initialize();

            Grasshopper.Instances.CanvasCreated += this.OnCanvasCreated;
            this.ApplicationVersion = new Version(Grasshopper.Versioning.Version.ToString());
        }
        catch
        {
            startUpLogger.AddError(_grasshopperInitializationFailed);
            throw;
        }
    }

    /// <summary>
    /// Registers event handlers when a new Grasshopper canvas is created.
    /// </summary>
    private void OnCanvasCreated(GH_Canvas canvas)
    {
        this.LoadGrasshopperLibrary();

        _activeCanvas = Grasshopper.Instances.ActiveCanvas;
        _activeCanvas.DocumentChanged += this.OnDocumentChanged;
    }

    /// <summary>
    /// Handles the event when objects are added to the Grasshopper document.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e"
    /// >The event data.
    /// </param>
    private void OnObjectsAdded(object sender, GH_DocObjectEventArgs e)
    {
        foreach (var ghDocumentObject in e.Objects)
        {
            this.HookPreviewExpired(ghDocumentObject);
        }
    }

    /// <summary>
    /// Handles the event when objects are deleted from the Grasshopper document.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// The event data.
    /// </param>
    private void OnObjectsDeleted(object sender, GH_DocObjectEventArgs e)
    {
        foreach (var ghDocumentObject in e.Objects)
        {
            this.UnhookPreviewExpired(ghDocumentObject);

            this.ObjectRemoved?.Invoke(this,
                new GrasshopperObjectModifiedEventArgs(ghDocumentObject));
        }
    }

    /// <summary>
    /// Subscribes to the PreviewExpired event of a Grasshopper document object.
    /// </summary>
    /// <param name="documentObject">
    /// The document object to subscribe to
    /// .</param>
    private void HookPreviewExpired(IGH_DocumentObject documentObject)
    {
        documentObject.ObjectChanged += this.OnGrasshopperObjectChanged;
    }

    /// <summary>
    /// Unsubscribes from the PreviewExpired event of a Grasshopper document object.
    /// </summary>
    /// <param name="documentObject">
    /// The document object to unsubscribe from.
    /// </param>
    private void UnhookPreviewExpired(IGH_DocumentObject documentObject)
    {
        documentObject.ObjectChanged -= this.OnGrasshopperObjectChanged;
    }

    /// <summary>
    /// Handles the ObjectChanged event for a Grasshopper document object.
    /// </summary>
    private void OnGrasshopperObjectChanged(IGH_DocumentObject sender, GH_ObjectChangedEventArgs e)
    {
        if (e.Type == GH_ObjectEventType.Preview)
        {
            this.PreviewExpired?.Invoke(this,
                new GrasshopperObjectModifiedEventArgs(sender));
        }
    }

    /// <summary>
    /// Subscribes to events in the specified Grasshopper document.
    /// </summary>
    /// <param name="document">
    /// The Grasshopper document to subscribe to.
    /// </param>
    private void AddDocumentSubscriptions(GH_Document document)
    {
        document.ObjectsAdded += this.OnObjectsAdded;
        document.ObjectsDeleted += this.OnObjectsDeleted;
        document.SolutionEnd += this.OnSolutionEnd;

        foreach (var ghDocumentObject in document.Objects)
        {
            this.HookPreviewExpired(ghDocumentObject);
        }

        _selectionTracker = new GrasshopperSelectionTracker(document);
        _selectionTracker.ObjectsSelected += this.OnObjectsSelected;
        _selectionTracker.ObjectsDeselected += this.OnObjectsDeselected;
    }

    /// <summary>
    /// Removes subscriptions to events in the current Grasshopper document.
    /// </summary>
    private void RemoveDocumentSubscriptions()
    {
        if (this.ActiveDoc == null) return;

        this.ActiveDoc.ObjectsAdded -= this.OnObjectsAdded;
        this.ActiveDoc.ObjectsDeleted -= this.OnObjectsDeleted;
        this.ActiveDoc.SolutionEnd -= this.OnSolutionEnd;

        foreach (var obj in this.ActiveDoc.Objects)
        {
            this.UnhookPreviewExpired(obj);
        }

        if (_selectionTracker == null) return;

        _selectionTracker.ObjectsSelected -= this.OnObjectsSelected;
        _selectionTracker.ObjectsDeselected -= this.OnObjectsDeselected;

        _selectionTracker.Dispose();
        _selectionTracker = null;
    }

    /// <summary>
    /// Triggers the preview expired event when the attributes of a Grasshopper document
    /// object change, for example when the component is selected or deselected. This
    /// ensures that the preview geometry is updated to reflect the selection state of
    /// the component.
    /// </summary>
    private void OnObjectsSelected(object sender, IGrasshopperSelectionEventArgs e)
    {
        this.ComponentSelectionChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Triggers the preview expired event when the attributes of a Grasshopper document
    /// object change, for example when the component is selected or deselected. This
    /// ensures that the preview geometry is updated to reflect the selection state of
    /// the component.
    /// </summary>
    private void OnObjectsDeselected(object sender, IGrasshopperSelectionEventArgs e)
    {
        this.ComponentSelectionChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Handles the event when a Grasshopper solution ends, this triggers the recalculation
    /// of the autocad previews.
    /// </summary>
    private void OnSolutionEnd(object sender, GH_SolutionEventArgs e)
    {
        foreach (var ghDocumentObject in e.Document.Objects)
        {
            if (ghDocumentObject is not IGH_PreviewObject { Hidden: false })
                continue;

            this.PreviewExpired?.Invoke(this,
                new GrasshopperObjectModifiedEventArgs(ghDocumentObject));
        }
    }

    /// <summary>
    /// Handles the event when the active Grasshopper document changes.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// The event data.
    /// </param>
    private void OnDocumentChanged(GH_Canvas sender, GH_CanvasDocumentChangedEventArgs e)
    {
        this.RemoveDocumentSubscriptions();

        this.ActiveDoc = e.NewDocument;

        if (this.ActiveDoc != null)
        {
            this.AddDocumentSubscriptions(this.ActiveDoc);
        }
    }

    /// <summary>
    /// Validates that the Grasshopper library is loaded into the Grasshopper component server.
    /// </summary>
    /// <param name="startUpLogger">
    /// The logger to record validation messages.
    /// </param>
    public void ValidateGrasshopperLibrary(IStartUpLogger startUpLogger)
    {
        this.LoadGrasshopper(startUpLogger);
    }

    /// <summary>
    /// Recomputes the Grasshopper solution in the active Grasshopper document.
    /// </summary>
    public void RecomputeSolution()
    {
        if (this.ActiveDoc is null) return;

        this.ActiveDoc.NewSolution(true);
    }

    /// <summary>
    /// Disables the Grasshopper solver, preventing solutions from being recomputed.
    /// </summary>
    public void DisableSolver()
    {
        Grasshopper.Kernel.GH_Document.EnableSolutions = false;
    }

    /// <summary>
    /// Enables the Grasshopper solver, allowing solutions to be recomputed.
    /// </summary>
    public void EnableSolver()
    {
        Grasshopper.Kernel.GH_Document.EnableSolutions = true;
    }

    /// <summary>
    /// Disposes all Civil 3D database objects held by Goo types in all open Grasshopper documents.
    /// This prevents "Forgot to call Dispose?" warnings and potential access violations.
    /// </summary>
    private void DisposeAllCivilGooObjects()
    {
        System.Diagnostics.Debug.WriteLine("GrasshopperInstance: Disposing Civil 3D Goo objects...");

        try
        {
            var documentCount = Grasshopper.Instances.DocumentServer.DocumentCount;
            System.Diagnostics.Debug.WriteLine($"  DocumentServer has {documentCount} document(s)");

            foreach (GH_Document document in Grasshopper.Instances.DocumentServer)
            {
                var objectCount = document.ObjectCount;
                System.Diagnostics.Debug.WriteLine($"  Document '{document.DisplayName}' has {objectCount} object(s)");

                foreach (var ghObject in document.Objects)
                {
                    // Components have output parameters that hold the data
                    if (ghObject is IGH_Component component)
                    {
                        foreach (var param in component.Params.Output)
                        {
                            this.DisposeParamData(param);
                        }
                    }
                    // Also check standalone params
                    else if (ghObject is IGH_Param param)
                    {
                        this.DisposeParamData(param);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DisposeAllCivilGooObjects failed: {ex.Message}");
        }

        System.Diagnostics.Debug.WriteLine("GrasshopperInstance: Civil 3D Goo objects disposed.");
    }

    /// <summary>
    /// Disposes all IDisposable objects held in a parameter's volatile data.
    /// </summary>
    private void DisposeParamData(IGH_Param param)
    {
        var dataCount = param.VolatileDataCount;
        if (dataCount > 0)
        {
            System.Diagnostics.Debug.WriteLine($"    Param '{param.Name}' has {dataCount} data item(s)");
        }

        foreach (var data in param.VolatileData.AllData(true))
        {
            if (data == null) continue;

            // Try to get the underlying value from the Goo
            object? valueToDispose = null;

            if (data is IGH_Goo goo)
            {
                valueToDispose = goo.ScriptVariable();
            }

            if (valueToDispose is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to dispose {valueToDispose.GetType().Name}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Shuts down the Grasshopper instance, releasing resources and removing
    /// subscriptions.
    /// </summary>
    public void Shutdown()
    {
        System.Diagnostics.Debug.WriteLine("=== GrasshopperInstance.Shutdown() START ===");

        // Dispose Civil 3D objects BEFORE removing document subscriptions
        this.DisposeAllCivilGooObjects();

        this.RemoveDocumentSubscriptions();

        if (_activeCanvas != null)
        {
            _activeCanvas.DocumentChanged -= this.OnDocumentChanged;
            _activeCanvas = null;
        }

        Grasshopper.Instances.CanvasCreated -= this.OnCanvasCreated;

        System.Diagnostics.Debug.WriteLine("=== GrasshopperInstance.Shutdown() END ===");
    }
}

