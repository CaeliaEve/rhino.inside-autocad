using System;
using Autodesk.AutoCAD.ApplicationServices;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Proactive environment detector for Autodesk Civil 3D runtime availability.
/// Ensures graceful fallback and friendly user alerts when running on standard AutoCAD.
/// </summary>
public static class CivilEnvironment
{
    private static bool? _isCivilHost;

    /// <summary>
    /// Gets a value indicating whether Civil 3D managed runtime and active document are available.
    /// </summary>
    public static bool IsAvailable
    {
        get
        {
            if (_isCivilHost.HasValue) return _isCivilHost.Value;

            try
            {
                // Check if CivilApplication type can be loaded and accessed
                var civilDoc = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;
                _isCivilHost = civilDoc != null;
            }
            catch
            {
                _isCivilHost = false;
            }

            return _isCivilHost.Value;
        }
    }

    /// <summary>
    /// Resets the cached detection state (useful when switching drawings).
    /// </summary>
    public static void ResetCache()
    {
        _isCivilHost = null;
    }
}
