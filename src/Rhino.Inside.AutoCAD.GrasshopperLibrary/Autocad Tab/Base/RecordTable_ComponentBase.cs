using Autodesk.AutoCAD.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using System.Collections;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A base class for Grasshopper components which return AutoCAD records. This class
/// provides common methods to get records by name or id, and to get all records
/// in the document. The specific type of record and the table they are stored
/// in are defined by the derived classes, no object are stored in this base class,
/// all the information is retrieved directly from the AutoCAD document when requested.
/// </summary>
public abstract class RecordTable_ComponentBase<TWrapper, TCad> : RhinoInsideAutocad_ComponentBase, IReferenceComponent
    where TWrapper : INamedDbObject
    where TCad : DBObject
{
    /// <summary>
    /// Passes the component name, nickname, description, category and subcategory to the base class constructor
    /// </summary>
    protected RecordTable_ComponentBase(string name, string nickname, string description,
        string category, string subCategory)
        : base(name, nickname, description, category, subCategory)
    {
    }

    /// <summary>
    /// A method to wrap an AutoCAD record in a wrapper class.
    /// </summary>
    protected abstract TWrapper Wrap(TCad cadObject);

    /// <summary>
    /// Returns the table of records in the AutoCAD document that this component uses.
    /// </summary>
    protected abstract IEnumerable GetTable(
        IAutocadTransactionManager transactionManagerWrapper);

    /// <summary>
    /// Returns the name of the AutoCAD record, used to compare with the input name
    /// when searching for a record by name.
    /// </summary>
    protected abstract string GetName(TCad cadObject);

    /// <summary>
    /// Returns the first records in the AutoCAD document which match the provided ObjectId. 
    /// </summary>
    protected bool TryGetById(IAutocadTransactionManager transactionManagerWrapper,
        IObjectId objectId, out TWrapper? wrapper)
    {
        wrapper = default;

        var transactionManager = transactionManagerWrapper.Unwrap();

        var table = this.GetTable(transactionManagerWrapper);

        var cadObjectId = objectId.Unwrap();

        foreach (ObjectId id in table)
        {
            if (id.OldIdPtr != cadObjectId.OldIdPtr) continue;

            var record = (TCad)transactionManager.GetObject(id, OpenMode.ForRead)!;

            wrapper = this.Wrap(record);

            return true;

        }

        return false;
    }

    /// <summary>
    /// Returns the first records in the AutoCAD document which match the provided name. 
    /// </summary>
    protected bool TryGetByName(
        IAutocadTransactionManager transactionManagerWrapper,
        string name, out TWrapper? wrapper)
    {
        wrapper = default;

        var transactionManager = transactionManagerWrapper.Unwrap();

        var table = this.GetTable(transactionManagerWrapper);

        foreach (ObjectId id in table)
        {
            var record = (TCad)transactionManager.GetObject(id, OpenMode.ForRead)!;

            if (this.GetName(record) != name) continue;

            wrapper = this.Wrap(record);

            return true;

        }

        return false;
    }

    /// <summary>
    /// Returns the all records in the AutoCAD document.
    /// </summary>
    protected List<TWrapper> GetAllRecords(IAutocadTransactionManager transactionManagerWrapper)
    {
        var wrappers = new List<TWrapper>();

        var transactionManager = transactionManagerWrapper.Unwrap();

        var table = this.GetTable(transactionManagerWrapper);

        foreach (ObjectId id in table)
        {
            var record = (TCad)transactionManager.GetObject(id, OpenMode.ForRead)!;

            var wrapper = this.Wrap(record);

            wrappers.Add(wrapper);
        }

        return wrappers;
    }

    /// <inheritdoc />
    public bool NeedsToBeExpired(IAutocadDocumentChange change)
    {
        foreach (var ghParam in this.Params.Output.OfType<IReferenceParam>())
        {
            if (ghParam.NeedsToBeExpired(change)) return true;
        }

        foreach (var changedObject in change)
        {
            if (changedObject.UnwrapObject() is TCad)
            {
                return true;
            }
        }

        return false;

    }
}