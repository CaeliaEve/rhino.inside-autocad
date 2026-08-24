using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Rhino.Inside.AutoCAD.Core.Host;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using CadEntity = Autodesk.AutoCAD.DatabaseServices.Entity;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <inheritdoc cref="IAutocadObjectPicker"/>
public class AutocadObjectPicker : IAutocadObjectPicker
{
    private readonly IAutocadDocument? _document;

    /// <summary>
    /// Constructs a new <see cref="IAutocadObjectPicker"/> instance.
    /// </summary>
    public AutocadObjectPicker()
    {
        var rhinoInsideApplication = (AutoCadHostContext.HostApplication as Rhino.Inside.AutoCAD.Core.Interfaces.IRhinoInsideAutoCadApplication);
        _document = rhinoInsideApplication?.RhinoInsideManager?.AutoCadInstance?.ActiveDocument;
    }

    /// <inheritdoc/>
    public IEntity? PickObject(IAutocadSelectionFilterWrapper filterWrapper, string message)
    {
        if (_document == null) return null;

        try
        {
            Application.MainWindow?.Focus();
        }
        catch { }

        var transactionManagerWrapper = _document.CreateTransactionManager();

        return transactionManagerWrapper.PerformTask(() =>
        {
            var entities = new List<IEntity>();
            var options = new PromptSelectionOptions()
            {
                AllowDuplicates = false,
                AllowSubSelections = true,
                ForceSubSelections = false,
                MessageForAdding = message,
                MessageForRemoval = message,
                SingleOnly = true,
            };

            var selectionFilter = filterWrapper.Unwrap();

            var promptSelectionResult = _document.Unwrap().Editor.GetSelection(options, selectionFilter);

            if (promptSelectionResult.Status != PromptStatus.OK) return null;

            var transaction = transactionManagerWrapper.Unwrap();

            var selectionSet = promptSelectionResult.Value;

            foreach (SelectedObject selectedObject in selectionSet)
            {
                if (selectedObject == null) continue;

                var entity = transaction.GetObject(selectedObject.ObjectId,
                    OpenMode.ForRead) as CadEntity;

                var wrappedEntity = new AutocadEntityWrapper(entity);

                entities.Add(wrappedEntity);
            }

            return entities.FirstOrDefault();

        });
    }

    /// <inheritdoc/>
    public IList<IEntity> PickObjects(IAutocadSelectionFilterWrapper filterWrapper, string message)
    {
        if (_document == null) return new List<IEntity>();

        try
        {
            Application.MainWindow?.Focus();
        }
        catch { }

        var transactionManagerWrapper = _document.CreateTransactionManager();

        return transactionManagerWrapper.PerformTask(() =>
        {
            var entities = new List<IEntity>();
            var options = new PromptSelectionOptions()
            {
                AllowDuplicates = false,
                AllowSubSelections = true,
                ForceSubSelections = false,
                MessageForAdding = message,
                MessageForRemoval = message,
                SingleOnly = false,
            };

            var selectionFilter = filterWrapper.Unwrap();

            var promptSelectionResult = _document.Unwrap().Editor
                .GetSelection(options, selectionFilter);

            if (promptSelectionResult.Status != PromptStatus.OK) return entities;

            var transaction = transactionManagerWrapper.Unwrap();

            var selectionSet = promptSelectionResult.Value;

            foreach (SelectedObject selectedObject in selectionSet)
            {
                if (selectedObject == null) continue;

                var entity = transaction.GetObject(selectedObject.ObjectId,
                    OpenMode.ForRead) as CadEntity;

                var wrappedEntity = new AutocadEntityWrapper(entity);

                entities.Add(wrappedEntity);
            }

            return entities;

        });
    }

    /// <inheritdoc/>
    public bool TryGetUpdatedObject(IObjectId objectId, out IEntity? entity)
    {
        if (_document == null)
        {
            entity = null;
            return false;
        }

        var transactionManagerWrapper = _document.CreateTransactionManager();

        entity = transactionManagerWrapper.PerformTask(() =>
        {
            if (objectId.IsValid == false) return null;
            try
            {
                var transaction = transactionManagerWrapper.Unwrap();

                var cadEntity = transaction.GetObject(objectId.Unwrap(),
                       OpenMode.ForRead) as CadEntity;

                return new AutocadEntityWrapper(cadEntity);
            }
            catch (Exception)
            {
                return null;
            }
        });

        return entity != null;
    }
}
