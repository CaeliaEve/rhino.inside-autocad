using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Provides extension methods for extracting labels from Civil 3D Alignments.
/// </summary>
public static class CivilLabelExtensions
{

    /// <summary>
    /// Creates the appropriate label wrapper based on the object type.
    /// </summary>
    public static ICivilFeatureLabel? CreateLabelWrapper(
        this FeatureLabel featureLabel,
        IAutocadTransactionManager transactionManager)
    {

        return featureLabel switch
        {
            AlignmentCurveLabel curve => new CivilAlignmentCurveLabelWrapper(curve),
            AlignmentSpiralLabel spiral => new CivilAlignmentSpiralLabelWrapper(spiral),
            AlignmentTangentLabel tangent => new CivilAlignmentTangentLabelWrapper(tangent),
            AlignmentPILabel pi => new CivilAlignmentPILabelWrapper(pi),
            AlignmentIndexedPILabel indexedPI => new CivilAlignmentIndexedPILabelWrapper(indexedPI),
            _ => throw new Exception("Feature Label type not supported")
        };

    }
}
