using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using System.Security.Cryptography;
using System.Text;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <inheritdoc cref="IInputSignatureBuilder"/>
public class InputSignatureBuilder : IInputSignatureBuilder
{
    private readonly StringBuilder _stringBuilder = new();
    private const char _separator = '|';

    /// <inheritdoc />
    public IInputSignatureBuilder Add(string? value)
    {
        _stringBuilder.Append(value ?? string.Empty);
        _stringBuilder.Append(_separator);
        return this;
    }

    /// <inheritdoc />
    public IInputSignatureBuilder Add(int value)
    {
        _stringBuilder.Append(value);
        _stringBuilder.Append(_separator);
        return this;
    }

    /// <inheritdoc />
    public IInputSignatureBuilder Add(double value, int decimals = 6)
    {
        _stringBuilder.Append(Math.Round(value, decimals));
        _stringBuilder.Append(_separator);
        return this;
    }

    /// <inheritdoc />
    public IInputSignatureBuilder Add(IObjectId? objectId)
    {
        _stringBuilder.Append(objectId?.Value ?? 0L);
        _stringBuilder.Append(_separator);
        return this;
    }

    /// <inheritdoc />
    public IInputSignatureBuilder AddCurve(Rhino.Geometry.Curve? curve)
    {
        if (curve == null)
        {
            _stringBuilder.Append("null");
            _stringBuilder.Append(_separator);
            return this;
        }

        var bbox = curve.GetBoundingBox(false);
        this.AddBoundingBox(bbox);

        _stringBuilder.Append(curve.Domain.T0.ToString("F6"));
        _stringBuilder.Append(',');
        _stringBuilder.Append(curve.Domain.T1.ToString("F6"));
        _stringBuilder.Append(',');
        _stringBuilder.Append(curve.Degree);
        _stringBuilder.Append(',');

        // Add control point count for NURBS curves
        if (curve is Rhino.Geometry.NurbsCurve nurbsCurve)
        {
            _stringBuilder.Append(nurbsCurve.Points.Count);
            _stringBuilder.Append(',');

            // Sample a few control points for more robust comparison
            var step = Math.Max(1, nurbsCurve.Points.Count / 5);
            for (var i = 0; i < nurbsCurve.Points.Count; i += step)
            {
                var pt = nurbsCurve.Points[i].Location;
                _stringBuilder.Append(pt.X.ToString("F4"));
                _stringBuilder.Append(',');
                _stringBuilder.Append(pt.Y.ToString("F4"));
                _stringBuilder.Append(',');
                _stringBuilder.Append(pt.Z.ToString("F4"));
                _stringBuilder.Append(',');
            }
        }
        else
        {
            // For non-NURBS curves, sample points along the curve
            _stringBuilder.Append("polyline,");
            var divisions = 10;
            var parameters = curve.DivideByCount(divisions, true);
            if (parameters != null)
            {
                foreach (var t in parameters)
                {
                    var pt = curve.PointAt(t);
                    _stringBuilder.Append(pt.X.ToString("F4"));
                    _stringBuilder.Append(',');
                    _stringBuilder.Append(pt.Y.ToString("F4"));
                    _stringBuilder.Append(',');
                    _stringBuilder.Append(pt.Z.ToString("F4"));
                    _stringBuilder.Append(',');
                }
            }
        }

        _stringBuilder.Append(_separator);
        return this;
    }

    /// <inheritdoc />
    public IInputSignatureBuilder AddMesh(Rhino.Geometry.Mesh? mesh)
    {
        if (mesh == null)
        {
            _stringBuilder.Append("null");
            _stringBuilder.Append(_separator);
            return this;
        }

        var bbox = mesh.GetBoundingBox(false);
        this.AddBoundingBox(bbox);

        _stringBuilder.Append(mesh.Vertices.Count);
        _stringBuilder.Append(',');
        _stringBuilder.Append(mesh.Faces.Count);
        _stringBuilder.Append(',');

        // Sample vertices for comparison (every Nth vertex based on size)
        var step = Math.Max(1, mesh.Vertices.Count / 10);
        for (var i = 0; i < mesh.Vertices.Count; i += step)
        {
            var pt = mesh.Vertices[i];
            _stringBuilder.Append(pt.X.ToString("F4"));
            _stringBuilder.Append(',');
            _stringBuilder.Append(pt.Y.ToString("F4"));
            _stringBuilder.Append(',');
            _stringBuilder.Append(pt.Z.ToString("F4"));
            _stringBuilder.Append(',');
        }

        _stringBuilder.Append(_separator);
        return this;
    }

    /// <summary>
    /// Adds a Rhino point to the signature.
    /// </summary>
    public IInputSignatureBuilder AddPoint(Rhino.Geometry.Point3d point)
    {
        _stringBuilder.Append(point.X.ToString("F6"));
        _stringBuilder.Append(',');
        _stringBuilder.Append(point.Y.ToString("F6"));
        _stringBuilder.Append(',');
        _stringBuilder.Append(point.Z.ToString("F6"));
        _stringBuilder.Append(_separator);
        return this;
    }

    /// <inheritdoc />
    public IInputSignatureBuilder AddGeometry(Rhino.Geometry.GeometryBase? geometry)
    {
        switch (geometry)
        {
            case null:
                _stringBuilder.Append("null");
                _stringBuilder.Append(_separator);
                return this;
            case Rhino.Geometry.Curve curve:
                return this.AddCurve(curve);
            case Rhino.Geometry.Mesh mesh:
                return this.AddMesh(mesh);
            case Rhino.Geometry.Point point:
                return this.AddPoint(point.Location);
        }

        _stringBuilder.Append(geometry.GetType().Name);
        _stringBuilder.Append(',');

        var bbox = geometry.GetBoundingBox(false);
        this.AddBoundingBox(bbox);

        if (geometry is Rhino.Geometry.Brep brep)
        {
            _stringBuilder.Append(brep.Faces.Count);
            _stringBuilder.Append(',');
            _stringBuilder.Append(brep.Edges.Count);
            _stringBuilder.Append(',');
            _stringBuilder.Append(brep.Vertices.Count);
            _stringBuilder.Append(',');

            // Sample vertices for comparison (every Nth vertex based on size)
            var step = Math.Max(1, brep.Vertices.Count / 10);
            for (var i = 0; i < brep.Vertices.Count; i += step)
            {
                var pt = brep.Vertices[i].Location;
                _stringBuilder.Append(pt.X.ToString("F4"));
                _stringBuilder.Append(',');
                _stringBuilder.Append(pt.Y.ToString("F4"));
                _stringBuilder.Append(',');
                _stringBuilder.Append(pt.Z.ToString("F4"));
                _stringBuilder.Append(',');
            }
        }

        _stringBuilder.Append(_separator);
        return this;
    }

    /// <inheritdoc />
    public IInputSignatureBuilder AddPoints(IList<Rhino.Geometry.Point3d>? points)
    {
        if (points == null || points.Count == 0)
        {
            _stringBuilder.Append("empty");
            _stringBuilder.Append(_separator);
            return this;
        }

        _stringBuilder.Append(points.Count);
        _stringBuilder.Append(',');

        foreach (var point in points)
        {
            _stringBuilder.Append(point.X.ToString("F6"));
            _stringBuilder.Append(',');
            _stringBuilder.Append(point.Y.ToString("F6"));
            _stringBuilder.Append(',');
            _stringBuilder.Append(point.Z.ToString("F6"));
            _stringBuilder.Append(',');
        }

        _stringBuilder.Append(_separator);
        return this;
    }

    /// <inheritdoc />
    public IInputSignatureBuilder AddScale(IAutocadScale scale)
    {
        _stringBuilder.Append(scale.X.ToString("F6"));
        _stringBuilder.Append(',');
        _stringBuilder.Append(scale.Y.ToString("F6"));
        _stringBuilder.Append(',');
        _stringBuilder.Append(scale.Z.ToString("F6"));
        _stringBuilder.Append(_separator);
        return this;
    }

    /// <inheritdoc />
    public IInputSignatureBuilder AddColor(IAutocadColor? color)
    {
        if (color == null)
        {
            _stringBuilder.Append("null");
        }
        else
        {
            var cadColor = color.Unwrap();

            _stringBuilder.Append(color.ColorIndex);
            _stringBuilder.Append(',');
            _stringBuilder.Append(cadColor.Red);
            _stringBuilder.Append(',');
            _stringBuilder.Append(cadColor.Green);
            _stringBuilder.Append(',');
            _stringBuilder.Append(cadColor.Blue);
        }
        _stringBuilder.Append(_separator);
        return this;
    }

    /// <inheritdoc />
    public IInputSignatureBuilder AddDoubles(IReadOnlyList<double>? values, int decimals = 6)
    {
        if (values == null || values.Count == 0)
        {
            _stringBuilder.Append("empty");
            _stringBuilder.Append(_separator);
            return this;
        }

        _stringBuilder.Append(values.Count);
        _stringBuilder.Append(',');

        foreach (var value in values)
        {
            _stringBuilder.Append(Math.Round(value, decimals));
            _stringBuilder.Append(',');
        }

        _stringBuilder.Append(_separator);
        return this;
    }

    /// <inheritdoc />
    public IInputSignatureBuilder AddScales(IReadOnlyList<IAutocadScale?>? scales)
    {
        if (scales == null || scales.Count == 0)
        {
            _stringBuilder.Append("empty");
            _stringBuilder.Append(_separator);
            return this;
        }

        _stringBuilder.Append(scales.Count);
        _stringBuilder.Append(',');

        foreach (var scale in scales)
        {
            if (scale == null)
            {
                _stringBuilder.Append("null");
            }
            else
            {
                _stringBuilder.Append(scale.X.ToString("F6"));
                _stringBuilder.Append(',');
                _stringBuilder.Append(scale.Y.ToString("F6"));
                _stringBuilder.Append(',');
                _stringBuilder.Append(scale.Z.ToString("F6"));
            }
            _stringBuilder.Append(',');
        }

        _stringBuilder.Append(_separator);
        return this;
    }

    /// <inheritdoc />
    public IInputSignatureBuilder AddObjectIds(IReadOnlyList<IObjectId?>? objectIds)
    {
        if (objectIds == null || objectIds.Count == 0)
        {
            _stringBuilder.Append("empty");
            _stringBuilder.Append(_separator);
            return this;
        }

        _stringBuilder.Append(objectIds.Count);
        _stringBuilder.Append(',');

        foreach (var objectId in objectIds)
        {
            _stringBuilder.Append(objectId?.Value ?? 0L);
            _stringBuilder.Append(',');
        }

        _stringBuilder.Append(_separator);
        return this;
    }

    /// <inheritdoc />
    public IInputSignatureBuilder AddColors(IReadOnlyList<IAutocadColor?>? colors)
    {
        if (colors == null || colors.Count == 0)
        {
            _stringBuilder.Append("empty");
            _stringBuilder.Append(_separator);
            return this;
        }

        _stringBuilder.Append(colors.Count);
        _stringBuilder.Append(',');

        foreach (var color in colors)
        {
            if (color == null)
            {
                _stringBuilder.Append("null");
            }
            else
            {
                var cadColor = color.Unwrap();

                _stringBuilder.Append(color.ColorIndex);
                _stringBuilder.Append(',');
                _stringBuilder.Append(cadColor.Red);
                _stringBuilder.Append(',');
                _stringBuilder.Append(cadColor.Green);
                _stringBuilder.Append(',');
                _stringBuilder.Append(cadColor.Blue);
            }
            _stringBuilder.Append(',');
        }

        _stringBuilder.Append(_separator);
        return this;
    }

    private void AddBoundingBox(Rhino.Geometry.BoundingBox bbox)
    {
        _stringBuilder.Append(bbox.Min.X.ToString("F4"));
        _stringBuilder.Append(',');
        _stringBuilder.Append(bbox.Min.Y.ToString("F4"));
        _stringBuilder.Append(',');
        _stringBuilder.Append(bbox.Min.Z.ToString("F4"));
        _stringBuilder.Append(',');
        _stringBuilder.Append(bbox.Max.X.ToString("F4"));
        _stringBuilder.Append(',');
        _stringBuilder.Append(bbox.Max.Y.ToString("F4"));
        _stringBuilder.Append(',');
        _stringBuilder.Append(bbox.Max.Z.ToString("F4"));
        _stringBuilder.Append(',');
    }

    /// <inheritdoc />
    public string Build()
    {
        var raw = _stringBuilder.ToString();

        // For large signatures, use MD5 hash to keep serialization reasonable
        if (raw.Length > 1000)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(hash);
        }

        return raw;
    }
}
