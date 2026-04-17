using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CadDbObject = Autodesk.AutoCAD.DatabaseServices.DBObject;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Provides extension methods for unwrapping Civil 3D interface wrappers to their underlying API types.
/// </summary>
/// <remarks>
/// This converter enables direct access to native Civil 3D objects when the abstraction layer
/// needs to be bypassed for advanced operations or Civil 3D API interop.
/// Usage: <c>var nativeLabelGroup = myLabelGroup.Unwrap();</c>
/// </remarks>
/// <seealso cref="InteropConverter"/>
public static class CivilInteropConverter
{
    /// <summary>
    /// Unwraps an <see cref="ICivilAlignmentLabelGroup"/> to its underlying Civil 3D <see cref="LabelGroup"/>.
    /// </summary>
    /// <param name="labelGroup">The label group wrapper to unwrap.</param>
    /// <returns>The native Civil 3D <see cref="LabelGroup"/> instance.</returns>
    public static AlignmentLabelGroup Unwrap(this CivilAlignmentLabelGroupWrapper labelGroup)
    {
        var wrapper = (AutocadWrapperDisposableBase<CadDbObject>)labelGroup;

        return (AlignmentLabelGroup)wrapper.AutocadObject;
    }
}
