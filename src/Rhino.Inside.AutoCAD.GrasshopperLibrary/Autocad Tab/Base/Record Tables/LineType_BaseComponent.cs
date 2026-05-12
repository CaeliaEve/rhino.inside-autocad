using Autodesk.AutoCAD.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using System.Collections;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A base class for Grasshopper components which depend on the AutoCAD Line types. This class
/// provides common methods to get Line types by name or id, and to get all Line types.
/// </summary>
public abstract class LineType_BaseComponent : RecordTable_ComponentBase<IAutocadLinetypeTableRecord, LinetypeTableRecord>
{
    protected LineType_BaseComponent(string name, string nickname, string description, string category, string subcategory)
        : base(name, nickname, description, category, subcategory)
    {
    }

    /// <inheritdoc />
    protected override IAutocadLinetypeTableRecord Wrap(LinetypeTableRecord cadObject)
    {
        return new AutocadLinetypeTableRecordWrapper(cadObject);
    }

    /// <inheritdoc />
    protected override IEnumerable GetTable(IAutocadTransactionManager transactionManagerWrapper)
    {
        var layerId = transactionManagerWrapper.LinetypeTableId.Unwrap();

        var transactionManager = transactionManagerWrapper.Unwrap();

        return (LinetypeTable)transactionManager
            .GetObject(layerId, OpenMode.ForRead);

    }

    /// <inheritdoc />
    protected override string GetName(LinetypeTableRecord cadObject)
    {
        return cadObject.Name;
    }
}