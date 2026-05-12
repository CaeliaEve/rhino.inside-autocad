using Autodesk.AutoCAD.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using System.Collections;
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