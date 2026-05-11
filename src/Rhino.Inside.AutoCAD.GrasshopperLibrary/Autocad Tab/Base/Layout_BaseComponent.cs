using Autodesk.AutoCAD.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using System.Collections;
using CadLayer = Autodesk.AutoCAD.DatabaseServices.LayerTableRecord;
using CadLayout = Autodesk.AutoCAD.DatabaseServices.Layout;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A base class for Grasshopper components which depend on the AutoCAD Layouts. This class
/// provides common methods to get layouts by name or id, and to get all layouts.
/// </summary>
public abstract class Layout_BaseComponent : RecordTable_ComponentBase<IAutocadLayout, CadLayout>
{
    protected Layout_BaseComponent(string name, string nickname, string description, string category, string subcategory)
        : base(name, nickname, description, category, subcategory)
    {
    }

    /// <inheritdoc />
    protected override IAutocadLayout Wrap(CadLayout cadObject)
    {
        return new AutocadLayoutWrapper(cadObject);
    }

    /// <inheritdoc />
    protected override IEnumerable GetTable(IAutocadTransactionManager transactionManagerWrapper)
    {
        var layoutId = transactionManagerWrapper.LayoutDictionaryId.Unwrap();

        var transactionManager = transactionManagerWrapper.Unwrap();

        var layouts = (DBDictionary)transactionManager
            .GetObject(layoutId, OpenMode.ForRead);

        var list = new List<ObjectId>();
        foreach (var entity in layouts)
        {
            list.Add(entity.Value);
        }

        return list;

    }

    /// <inheritdoc />
    protected override string GetName(CadLayout cadObject)
    {
        return cadObject.LayoutName;
    }
}

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

/// <summary>
/// A base class for Grasshopper components which depend on the AutoCAD Layers. This class
/// provides common methods to get Layers by name or id, and to get all Layers.
/// </summary>
public abstract class Layer_BaseComponent : RecordTable_ComponentBase<IAutocadLayerTableRecord, CadLayer>
{
    protected Layer_BaseComponent(string name, string nickname, string description, string category, string subcategory)
        : base(name, nickname, description, category, subcategory)
    {
    }

    /// <inheritdoc />
    protected override IAutocadLayerTableRecord Wrap(CadLayer cadObject)
    {
        return new AutocadLayerTableRecordWrapper(cadObject);
    }

    /// <inheritdoc />
    protected override IEnumerable GetTable(IAutocadTransactionManager transactionManagerWrapper)
    {
        var layerId = transactionManagerWrapper.LayerTableId.Unwrap();

        var transactionManager = transactionManagerWrapper.Unwrap();

        return (LayerTable)transactionManager
            .GetObject(layerId, OpenMode.ForRead);

    }

    /// <inheritdoc />
    protected override string GetName(CadLayer cadObject)
    {
        return cadObject.Name;
    }
}

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