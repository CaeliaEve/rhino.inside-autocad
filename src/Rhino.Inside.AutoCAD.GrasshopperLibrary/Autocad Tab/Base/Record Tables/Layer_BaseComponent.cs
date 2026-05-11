using Autodesk.AutoCAD.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using System.Collections;
using CadLayer = Autodesk.AutoCAD.DatabaseServices.LayerTableRecord;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

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
