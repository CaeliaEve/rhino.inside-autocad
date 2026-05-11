namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Provides transactional access to AutoCAD database objects for read and write operations.
/// </summary>
/// <remarks>
/// Wraps the AutoCAD TransactionManager to decouple the Core library from AutoCAD types.
/// All database modifications in AutoCAD must occur within a transaction context. Obtain
/// an instance via <see cref="IAutocadDocument.CreateTransation"/> and use it to open
/// database objects for reading or writing. The transaction is committed or aborted
/// when disposed.
/// </remarks>
/// <seealso cref="IAutocadBlockTableRecord"/>
public interface IAutocadTransactionManager : IDisposable
{
    /// <summary>
    /// Gets the <see cref="IObjectId"/> of the document's block table.
    /// </summary>
    /// <remarks>
    /// The block table contains all block definitions in the drawing, including
    /// model space, paper space layouts, and user-defined blocks.
    /// </remarks>
    /// <seealso cref="IAutocadBlockTableRecord"/>
    IObjectId BlockTableId { get; }

    /// <summary>
    /// Gets the <see cref="IObjectId"/> of the document's Layout Dictionary.
    /// </summary>
    IObjectId LayoutDictionaryId { get; }

    /// <summary>
    /// Gets the <see cref="IObjectId"/> of the document's Layer table.
    /// </summary>
    IObjectId LayerTableId { get; }

    /// <summary>
    /// Gets the <see cref="IObjectId"/> of the document's Line Type table.
    /// </summary>
    IObjectId LinetypeTableId { get; }

    /// <summary>
    /// Gets the <see cref="IObjectId"/> of the document's app registration table.
    /// </summary>
    IObjectId RegAppTableId { get; }

    /// <summary>
    /// Opens the model space block table record for reading or writing.
    /// </summary>
    /// <param name="openForWrite">
    /// Set to <c>true</c> to open for write access (required for adding or modifying entities);
    /// <c>false</c> for read-only access. Defaults to <c>false</c>.
    /// </param>
    /// <returns>
    /// The <see cref="IAutocadBlockTableRecord"/> representing model space.
    /// </returns>
    /// <remarks>
    /// Model space is the primary drawing area where most entities reside. Use write
    /// access when adding new entities or modifying existing geometry.
    /// </remarks>
    IAutocadBlockTableRecord GetModelSpace(bool openForWrite = false);

    /// <summary>
    /// Persists the database, this can be used to save XData.
    /// </summary>
    /// <remarks>
    /// Skipped for read-only documents or unsaved documents without a file path.
    /// </remarks>
    void SaveDatabase(IAutocadDocument document);

    /// <summary>
    /// Executes a function within a database transaction and returns the result.
    /// </summary>
    /// <typeparam name="T">
    /// The type of value returned by the transaction function.
    /// </typeparam>
    /// <param name="function">
    /// The function to execute within the transaction scope.
    /// </param>
    /// <param name="abort">
    /// When <c>true</c>, aborts the transaction to roll back changes. Useful for
    /// read operations that temporarily modify the database to obtain data.
    /// </param>
    /// <returns>
    /// The value returned by <paramref name="function"/>.
    /// </returns>
    /// <remarks>
    /// All database modifications in AutoCAD must occur within a transaction.
    /// By default, transactions are committed. Set <paramref name="abort"/> to
    /// <c>true</c> when reading data that requires temporary modifications.
    /// </remarks>
    T PerformTask<T>(Func<T> function, bool abort = false);
}