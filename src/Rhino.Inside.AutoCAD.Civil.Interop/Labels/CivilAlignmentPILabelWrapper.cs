using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wrapper for Civil 3D Alignment PI (Point of Intersection) labels.
/// </summary>
public class CivilAlignmentPILabelWrapper : CivilFeatureLabelWrapperBase<AlignmentPILabel>, ICivilAlignmentPILabel
{
    public CivilAlignmentPILabelWrapper(AlignmentPILabel label)
        : base(label) { }
}

