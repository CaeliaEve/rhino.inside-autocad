using Autodesk.AutoCAD.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Rhino.Inside.AutoCAD.Interop;

/// <inheritdoc cref="IBrepConverterRunner"/>
public class BrepConverterRunner : IBrepConverterRunner
{
    private const string _rhinoInsideAutocadFolder = InteropConstants.RhinoInsideAutocadFolder;
    private const string _convertersFolder = InteropConstants.ConvertersFolder;
    private const string _rhinoFileName = InteropConstants.RhinoFileName;
    private const string _importCommand = InteropConstants.ImportCommand;
    private const string _brepConversionErrorMessage = InteropConstants.BrepConversionErrorMessage;

    /// <summary>
    /// Queue of pending <see cref="IBrepConverterRequest"/> items awaiting conversion.
    /// </summary>
    private readonly Queue<IBrepConverterRequest> _requests = new Queue<IBrepConverterRequest>();

    /// <summary>
    /// Converts a Rhino Brep geometry to one or more AutoCAD <see cref="Solid3d"/> objects.
    /// </summary>
    /// <param name="brepRequest">
    /// The conversion request containing the Brep geometry and callback delegate.
    /// </param>
    /// <returns>
    /// An <see cref="IBrepConverterResult"/> containing the converted <see cref="Solid3d"/> objects.
    /// Returns an empty result if the conversion fails.
    /// </returns>
    /// <remarks>
    /// This method writes the Brep to a temporary .3dm file and uses AutoCAD's IMPORT command
    /// to bring it into the active document. The imported geometry is then cloned and returned.
    /// Typically, produces a single solid, but complex Breps may result in multiple solids
    /// depending on the import processing.
    /// </remarks>
    /// <seealso cref="IBrepConverterRequest"/>
    /// <seealso cref="IBrepConverterResult"/>
    private IBrepConverterResult ToAutoCadType(IBrepConverterRequest brepRequest)
    {
        var activeDocument = Application.DocumentManager.MdiActiveDocument;

        var editor = activeDocument.Editor;

        var convertedSolids = new List<Solid3d>();

        try
        {
            var tempDirectory = Path.GetTempPath();

            var converterDirectory = Path.Combine(tempDirectory, _rhinoInsideAutocadFolder, _convertersFolder);

            Directory.CreateDirectory(converterDirectory);

            var rhinoFilePath = Path.Combine(converterDirectory, _rhinoFileName);

            var writeSucceeded = Rhino.FileIO.File3dm.WriteOneObject(rhinoFilePath, brepRequest.BrepToConvert);

            if (!File.Exists(rhinoFilePath) || !writeSucceeded)
            {
                return new BrepConverterResult(convertedSolids);
            }

            editor.Command(_importCommand, rhinoFilePath, "");

            var selectionResult = editor.SelectLast();

            var selectedObjects = selectionResult?.Value;

            var transaction = activeDocument.Database.TransactionManager.StartTransaction();

            for (var index = 0; index < selectedObjects!.Count; index++)
            {
                var selectedEntity = selectedObjects[index];

                var importedObject = transaction.GetObject(selectedEntity.ObjectId, OpenMode.ForWrite);

                var clonedObject = importedObject.Clone();

                if (clonedObject is not Solid3d solid) continue;

                convertedSolids.Add(solid);
            }
            transaction.Commit();
        }
        catch (System.Exception ex)
        {
            editor.WriteMessage($"{_brepConversionErrorMessage}{ex.Message}");
        }
        return new BrepConverterResult(convertedSolids);
    }

    /// <inheritdoc />
    public void Run()
    {
        while (_requests.Count > 0)
        {
            var request = _requests.Dequeue();

            var result = this.ToAutoCadType(request);

            _ = request.Callback.Invoke(result);
        }
    }

    /// <inheritdoc />
    public void EnqueueRequest(IBrepConverterRequest request)
    {
        _requests.Enqueue(request);
    }
}