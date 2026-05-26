using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wrapper for Civil 3D Alignment Spiral labels.
/// </summary>
public class CivilAlignmentSpiralLabelWrapper : CivilFeatureLabelWrapperBase<AlignmentSpiralLabel>, ICivilAlignmentSpiralLabel
{
    public CivilAlignmentSpiralLabelWrapper(AlignmentSpiralLabel label)
        : base(label) { }
}