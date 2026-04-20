using Autodesk.AutoCAD.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Interop;

/// <inheritdoc cref="IAutocadDocumentId"/>
public class AutocadDocumentId : IAutocadDocumentId
{
    private const string _applicationName = InteropConstants.ApplicationName;
    private const short _applicationNameKey = XRecordKeys.ApplicationNameKey;
    private const short _documentIdKey = XRecordKeys.DocumentIdKey;

    /// <summary>
    /// The registered Id of this document.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Constructs a new <see cref="IAutocadDocumentId"/>
    /// </summary>
    public AutocadDocumentId(IAutocadDocument document)
    {
        this.Register(document);

        if (this.TryGetExistingId(document, out var documentId) == false)
        {
            documentId = this.CreateNewId(document);
        }

        this.Id = documentId;
    }

    /// <summary>
    /// Retrieves the document's unique identifier from model space XData if it already
    /// exists, if not it returns false and an empty guid.
    /// </summary>
    private bool TryGetExistingId(IAutocadDocument document, out Guid id)
    {
        var transactionManagerWrapper = document.CreateTransactionManager();

        id = transactionManagerWrapper.PerformTask(() =>
        {
            var blockModelSpace = transactionManagerWrapper.GetModelSpace().Unwrap();

            var xData = blockModelSpace.XData == null
                ? new ResultBuffer()
                : blockModelSpace.XData;

            var documentIdKey = (short)_documentIdKey;

            var typedValues = xData.AsArray().Where(v => v.TypeCode == documentIdKey);

            var documentGuid = Guid.Empty;
            foreach (var typedValue in typedValues)
            {
                if (Guid.TryParse(typedValue.Value.ToString(), out documentGuid))
                    break;
            }

            return documentGuid;

        });

        return id.Equals(Guid.Empty);
    }

    /// <summary>
    /// Creates a document's unique identifier and stores it in the model space XData.
    /// </summary>
    private Guid CreateNewId(IAutocadDocument document)
    {
        var transactionManagerWrapper = document.CreateTransactionManager();

        return transactionManagerWrapper.PerformTask(() =>
        {
            var blockModelSpace = transactionManagerWrapper.GetModelSpace().Unwrap();

            var xData = blockModelSpace.XData == null
                ? new ResultBuffer()
                : blockModelSpace.XData;

            var idKey = (short)_documentIdKey;

            var documentId = Guid.NewGuid();

            xData.Add(new Autodesk.AutoCAD.DatabaseServices.TypedValue((short)_applicationNameKey, _applicationName));
            xData.Add(new Autodesk.AutoCAD.DatabaseServices.TypedValue(idKey, documentId.ToString()));

            blockModelSpace.UpgradeOpen();

            blockModelSpace.XData = xData;

            transactionManagerWrapper.SaveDatabase(document);

            return documentId;

        });

    }

    /// <summary>
    /// Registers Rhino.Inside.AutoCAD in the <see cref="RegAppTable"/>.
    /// </summary>
    /// <remarks>
    /// Required before writing XData to the database.
    /// </remarks>
    private void Register(IAutocadDocument document)
    {
        var transactionManagerWrapper = document.CreateTransactionManager();

        _ = transactionManagerWrapper.PerformTask(() =>
        {
            var transaction = transactionManagerWrapper.Unwrap();

            var regAppTableId = transactionManagerWrapper.RegAppTableId.Unwrap();

            var regAppTable = (RegAppTable)transaction.GetObject(regAppTableId, OpenMode.ForRead);

            if (regAppTable.Has(_applicationName)) return true;

            regAppTable.UpgradeOpen();

            var regAppTableRecord = new RegAppTableRecord();

            regAppTableRecord.Name = _applicationName;

            regAppTable.Add(regAppTableRecord);

            transaction.AddNewlyCreatedDBObject(regAppTableRecord, true);

            return true;

        });
    }
}