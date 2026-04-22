using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wrapper for Civil 3D Alignment Curve labels.
/// </summary>
public class CivilAlignmentCurveLabelWrapper : CivilFeatureLabelWrapperBase<AlignmentCurveLabel>, ICivilAlignmentCurveLabel
{
    public CivilAlignmentCurveLabelWrapper(AlignmentCurveLabel label)
        : base(label)
    {

    }
}