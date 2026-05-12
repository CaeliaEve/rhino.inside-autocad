using Autodesk.AutoCAD.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using System.Collections;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A base class for Grasshopper components which depend on the AutoCAD Blocks. This class
/// provides common methods to get Blocks by name or id, and to get all Blocks.
/// </summary>
public abstract class Block_BaseComponent : RecordTable_ComponentBase<IAutocadBlockTableRecord, BlockTableRecord>
{
    protected Block_BaseComponent(string name, string nickname, string description, string category, string subcategory)
        : base(name, nickname, description, category, subcategory)
    {
    }

    /// <inheritdoc />
    protected override IAutocadBlockTableRecord Wrap(BlockTableRecord cadObject)
    {
        return new AutocadBlockTableRecordWrapper(cadObject);
    }

    /// <inheritdoc />
    protected override IEnumerable GetTable(IAutocadTransactionManager transactionManagerWrapper)
    {
        var blockTableId = transactionManagerWrapper.BlockTableId;

        var transactionManager = transactionManagerWrapper.Unwrap();

        return (BlockTable)transactionManager.GetObject(blockTableId.Unwrap(), OpenMode.ForRead)!;
    }

    /// <inheritdoc />
    protected override string GetName(BlockTableRecord cadObject)
    {
        return cadObject.Name;
    }
}