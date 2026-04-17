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
    /// Creates a label group wrapper from a Civil 3D LabelGroup.
    /// </summary>
    public static CivilAlignmentLabelGroupWrapperBase? CreateLabelGroupWrapper(this LabelGroup labelGroup)
    {

        return labelGroup switch
        {
            AlignmentCantLabelGroup cant => new CivilAlignmentCantLabelGroupWrapper(cant.StyleName, (int)cant.SubEntityCount),
            AlignmentDesignSpeedLabelGroup designSpeed => new CivilAlignmentDesignSpeedLabelGroupWrapper(designSpeed.StyleName, (int)designSpeed.SubEntityCount),
            AlignmentGeometryPointLabelGroup geometryPoint => new CivilAlignmentGeometryPointLabelGroupWrapper(geometryPoint.StyleName, (int)geometryPoint.SubEntityCount),
            AlignmentStationEquationLabelGroup stationEquation => new CivilAlignmentStationEquationLabelGroupWrapper(stationEquation.StyleName, (int)stationEquation.SubEntityCount),
            AlignmentStationLabelGroup station => new CivilAlignmentStationLabelGroupWrapper(station.StyleName, (int)station.SubEntityCount),
            AlignmentSuperelevationLabelGroup superElevationLabelGroup => new CivilAlignmentSuperelevationLabelGroupWrapper(superElevationLabelGroup.StyleName, (int)superElevationLabelGroup.SubEntityCount),
            AlignmentVerticalGeometryPointLabelGroup verticalGeometryPoint => new CivilAlignmentVerticalGeometryPointLabelGroupWrapper(verticalGeometryPoint.StyleName, (int)verticalGeometryPoint.SubEntityCount),
            _ => throw new Exception("Alignment Label Group type not supported")
        };
    }

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
