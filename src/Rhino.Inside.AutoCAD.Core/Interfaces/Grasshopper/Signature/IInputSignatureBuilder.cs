namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Builds a signature string from component inputs for change detection.
/// Used by Create components to determine if inputs have changed since last solve.
/// </summary>
public interface IInputSignatureBuilder
{
    /// <summary>
    /// Adds a string value to the signature.
    /// </summary>
    IInputSignatureBuilder Add(string? value);

    /// <summary>
    /// Adds an integer value to the signature.
    /// </summary>
    IInputSignatureBuilder Add(int value);

    /// <summary>
    /// Adds a double value to the signature with specified decimal precision.
    /// </summary>
    IInputSignatureBuilder Add(double value, int decimals = 6);

    /// <summary>
    /// Adds an ObjectId reference to the signature using its handle value.
    /// </summary>
    IInputSignatureBuilder Add(IObjectId? objectId);

    /// <summary>
    /// Adds a Rhino curve geometry to the signature.
    /// Uses bounding box corners, domain, degree, and control point count for comparison.
    /// </summary>
    IInputSignatureBuilder AddCurve(Rhino.Geometry.Curve? curve);

    /// <summary>
    /// Adds a Rhino mesh geometry to the signature.
    /// Uses bounding box corners, vertex count, face count, and sampled vertex positions.
    /// </summary>
    IInputSignatureBuilder AddMesh(Rhino.Geometry.Mesh? mesh);

    /// <summary>
    /// Adds a Rhino point to the signature.
    /// </summary>
    IInputSignatureBuilder AddPoint(Rhino.Geometry.Point3d point);

    /// <summary>
    /// Adds a list of Rhino points to the signature.
    /// </summary>
    IInputSignatureBuilder AddPoints(IList<Rhino.Geometry.Point3d>? points);

    /// <summary>
    /// Adds a scale value to the signature.
    /// </summary>
    IInputSignatureBuilder AddScale(IAutocadScale scale);

    /// <summary>
    /// Adds a color to the signature.
    /// </summary>
    IInputSignatureBuilder AddColor(IAutocadColor? color);

    /// <summary>
    /// Builds and returns the final signature string.
    /// For large inputs, returns an MD5 hash; otherwise returns the raw string.
    /// </summary>
    string Build();
}