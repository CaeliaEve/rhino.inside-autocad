using CadPoint3d = Autodesk.AutoCAD.Geometry.Point3d;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// An equality comparer for CadPoint3d that considers two points equal
/// if they are within a small tolerance of each other.
/// </summary>
public class CadPoint3dComparer : IEqualityComparer<CadPoint3d>
{
    public static readonly CadPoint3dComparer Instance = new CadPoint3dComparer();

    private const double Tolerance = GeometryConstants.ZeroTolerance;

    public bool Equals(CadPoint3d a, CadPoint3d b)
    {
        return Math.Abs(a.X - b.X) < Tolerance &&
               Math.Abs(a.Y - b.Y) < Tolerance &&
               Math.Abs(a.Z - b.Z) < Tolerance;
    }

    public int GetHashCode(CadPoint3d p)
    {
        // Round to tolerance bucket before hashing so that points
        // which compare equal always produce the same hash code
        var x = Math.Round(p.X / Tolerance) * Tolerance;
        var y = Math.Round(p.Y / Tolerance) * Tolerance;
        var z = Math.Round(p.Z / Tolerance) * Tolerance;

        return HashCode.Combine(x, y, z);
    }
}