namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents volume statistics extracted from a Civil 3D TIN Volume Surface.
/// </summary>
/// <remarks>
/// Volume properties provide cut/fill analysis data including raw (unadjusted)
/// volumes, adjustment factors, and adjusted volumes.
/// </remarks>
public interface ICivilTinVolumeSurfaceProperties
{
    /// <summary>
    /// Gets the raw cut volume before applying the cut factor.
    /// </summary>
    double UnadjustedCutVolume { get; }

    /// <summary>
    /// Gets the raw fill volume before applying the fill factor.
    /// </summary>
    double UnadjustedFillVolume { get; }

    /// <summary>
    /// Gets the raw net volume (unadjusted cut - unadjusted fill).
    /// </summary>
    double UnadjustedNetVolume { get; }

    /// <summary>
    /// Gets the cut volume adjustment factor.
    /// </summary>
    double CutFactor { get; }

    /// <summary>
    /// Gets the fill volume adjustment factor.
    /// </summary>
    double FillFactor { get; }

    /// <summary>
    /// Gets the adjusted cut volume (raw * factor).
    /// </summary>
    double AdjustedCutVolume { get; }

    /// <summary>
    /// Gets the adjusted fill volume (raw * factor).
    /// </summary>
    double AdjustedFillVolume { get; }

    /// <summary>
    /// Gets the adjusted net volume (adjusted cut - adjusted fill).
    /// </summary>
    double AdjustedNetVolume { get; }
}
