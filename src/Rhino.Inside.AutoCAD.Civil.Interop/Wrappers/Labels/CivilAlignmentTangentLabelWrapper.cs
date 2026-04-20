using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wrapper for Civil 3D Alignment Tangent labels.
/// </summary>
public class CivilAlignmentTangentLabelWrapper : CivilFeatureLabelWrapperBase<AlignmentTangentLabel>, ICivilAlignmentTangentLabel
{
    public CivilAlignmentTangentLabelWrapper(AlignmentTangentLabel label)
        : base(label) { }
}

