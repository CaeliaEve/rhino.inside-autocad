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
    /// Adds any Rhino geometry to the signature, dispatching to the most specific
    /// method available (curve, mesh, point). Breps use bounding box corners,
    /// face/edge/vertex counts, and sampled vertex positions; other geometry types
    /// fall back to type name and bounding box corners.
    /// </summary>
    IInputSignatureBuilder AddGeometry(Rhino.Geometry.GeometryBase? geometry);

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
    /// Adds a list of double values to the signature with specified decimal precision.
    /// </summary>
    IInputSignatureBuilder AddDoubles(IReadOnlyList<double>? values, int decimals = 6);

    /// <summary>
    /// Adds a list of scale values to the signature.
    /// </summary>
    IInputSignatureBuilder AddScales(IReadOnlyList<IAutocadScale?>? scales);

    /// <summary>
    /// Adds a list of ObjectId references to the signature using their handle values.
    /// </summary>
    IInputSignatureBuilder AddObjectIds(IReadOnlyList<IObjectId?>? objectIds);

    /// <summary>
    /// Adds a list of colors to the signature.
    /// </summary>
    IInputSignatureBuilder AddColors(IReadOnlyList<IAutocadColor?>? colors);

    /// <summary>
    /// Builds and returns the final signature string.
    /// For large inputs, returns an MD5 hash; otherwise returns the raw string.
    /// </summary>
    string Build();
}