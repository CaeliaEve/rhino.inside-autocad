using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using RhinoPoint3d = Rhino.Geometry.Point3d;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Provides extension methods for converting Civil 3D Assembly types to Rhino geometry types.
/// </summary>
public static class CivilAssemblyExtensions
{
    /// <summary>
    /// Extracts all subassemblies from a Civil 3D Assembly as property wrappers.
    /// </summary>
    /// <param name="assembly">The Civil 3D Assembly to extract subassemblies from.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A list of subassembly property wrappers.</returns>
    public static List<CivilSubassemblyPropertiesWrapper> GetSubassemblies(
        this Assembly assembly,
        IAutocadTransactionManager transactionManager)
    {
        var subassemblies = new List<CivilSubassemblyPropertiesWrapper>();

        var transaction = transactionManager.Unwrap();
        try
        {
            foreach (var group in assembly.Groups)
            {
                foreach (ObjectId subassemblyId in group.GetSubassemblyIds())
                {
                    var subassembly = transaction.GetObject(subassemblyId, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead) as Subassembly;
                    var wrapper = new CivilSubassemblyPropertiesWrapper(subassembly);
                    subassemblies.Add(wrapper);
                }
            }
        }
        catch
        {
            // Return empty list if subassembly extraction fails
        }

        return subassemblies;
    }

    /// <summary>
    /// Converts the origin location of a Civil 3D Assembly to a Rhino Point3d.
    /// </summary>
    /// <param name="assembly">The Civil 3D Assembly to get location from.</param>
    /// <returns>A Rhino Point3d representing the assembly's origin location.</returns>
    public static RhinoPoint3d ToRhinoPoint(this Assembly assembly)
    {
        try
        {
            var location = assembly.Location;
            return new RhinoPoint3d(
                UnitConverter.ToRhinoLength(location.X),
                UnitConverter.ToRhinoLength(location.Y),
                UnitConverter.ToRhinoLength(location.Z));
        }
        catch
        {
            return RhinoPoint3d.Origin;
        }
    }

    /// <summary>
    /// Extracts assembly groups from a Civil 3D Assembly.
    /// </summary>
    /// <param name="assembly">The Civil 3D Assembly to extract groups from.</param>
    /// <returns>A list of assembly group names.</returns>
    public static List<string> GetGroupNames(this Assembly assembly)
    {
        var groupNames = new List<string>();

        try
        {
            foreach (var group in assembly.Groups)
            {
                groupNames.Add(group.Name ?? "Unnamed Group");
            }
        }
        catch
        {
            // Return empty list if group extraction fails
        }

        return groupNames;
    }
}
