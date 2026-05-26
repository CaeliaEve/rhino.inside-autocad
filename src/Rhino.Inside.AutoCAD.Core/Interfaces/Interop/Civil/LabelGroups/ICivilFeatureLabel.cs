using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Base interface for all Civil 3D Alignment labels.
/// </summary>
/// <remarks>
/// This interface provides common properties shared by all alignment label types
/// including curve labels, spiral labels, tangent labels, and PI labels.
/// </remarks>
public interface ICivilFeatureLabel
{
    /// <summary>
    /// Gets the location of the label as a Rhino Point3d.
    /// </summary>
    Point3d Location { get; }

    /// <summary>
    /// Gets the specific type of alignment label.
    /// </summary>
    string LabelType { get; }

    /// <summary>
    /// Extracts the text content from a label using its text component IDs.
    /// </summary>
    List<IEntity> ExtractTextEntities(
       IAutocadTransactionManager transactionManager);

    /// <summary>
    /// Gets the style name from a style ObjectId.
    /// </summary>
    string GetStyleName(IAutocadTransactionManager transactionManager);
}

/// <summary>
/// Interface for Civil 3D Alignment Curve labels.
/// </summary>
public interface ICivilAlignmentCurveLabel : ICivilFeatureLabel
{
}

/// <summary>
/// Interface for Civil 3D Alignment Spiral labels.
/// </summary>
public interface ICivilAlignmentSpiralLabel : ICivilFeatureLabel
{
}

/// <summary>
/// Interface for Civil 3D Alignment Tangent labels.
/// </summary>
public interface ICivilAlignmentTangentLabel : ICivilFeatureLabel
{
}

/// <summary>
/// Interface for Civil 3D Alignment PI (Point of Intersection) labels.
/// </summary>
public interface ICivilAlignmentPILabel : ICivilFeatureLabel
{
}

/// <summary>
/// Interface for Civil 3D Alignment Indexed PI labels.
/// </summary>
public interface ICivilAlignmentIndexedPILabel : ICivilFeatureLabel
{
}
