using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// A comparer which treats two <see cref="GH_AssemblyInfo"/> as the same library when they
/// describe the same assembly.
/// </summary>
/// <remarks>
/// Compares the assembly simple name on both sides. Comparing one side's assembly name
/// against the other's <see cref="GH_AssemblyInfo.Name"/> compares an assembly name against
/// the plugin's display name - "Rhino.Inside.AutoCAD.GrasshopperLibrary" against
/// "Rhino.Inside.AutoCAD Plugin" - which never matches, making every "is it already
/// registered?" check answer no.
/// </remarks>
public class GH_AssemblyInfoStubComparer : IEqualityComparer<GH_AssemblyInfo>
{
    /// <summary>
    /// Determines whether the specified assemblies are equal by comparing the names of the
    /// assemblies they describe.
    /// </summary>
    /// <param name="x">The first assembly to compare.</param>
    /// <param name="y">The second assembly to compare.</param>
    /// <returns>True if they describe the same assembly; otherwise, false.</returns>
    public bool Equals(GH_AssemblyInfo? x, GH_AssemblyInfo? y)
    {
        var xName = GetAssemblyName(x);
        var yName = GetAssemblyName(y);

        if (xName == null || yName == null)
            return false;

        return string.Equals(xName, yName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns a hash code for the specified assembly based on the name of the assembly it
    /// describes, so that it agrees with <see cref="Equals"/>.
    /// </summary>
    /// <param name="obj">The assembly for which a hash code is to be returned.</param>
    /// <returns>A hash code for the assembly name.</returns>
    public int GetHashCode(GH_AssemblyInfo obj)
    {
        var name = GetAssemblyName(obj);

        return name == null
            ? 0
            : StringComparer.OrdinalIgnoreCase.GetHashCode(name);
    }

    /// <summary>
    /// Returns the simple name of the assembly the given info describes, or null when it
    /// describes no assembly.
    /// </summary>
    /// <param name="assemblyInfo">The assembly info to read.</param>
    private static string? GetAssemblyName(GH_AssemblyInfo? assemblyInfo)
    {
        return assemblyInfo?.Assembly?.GetName().Name;
    }
}
