using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wrapper for Civil 3D Alignment Indexed PI labels.
/// </summary>
public class CivilAlignmentIndexedPILabelWrapper : CivilFeatureLabelWrapperBase<AlignmentIndexedPILabel>, ICivilAlignmentIndexedPILabel
{
    public CivilAlignmentIndexedPILabelWrapper(AlignmentIndexedPILabel label)
        : base(label) { }
}
