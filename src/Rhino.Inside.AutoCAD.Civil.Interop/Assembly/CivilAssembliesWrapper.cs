using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Core.Interfaces.Assemblies;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

public class CivilAssembliesWrapper : AutocadEntityWrapper, ICivilAssemblies
{
    private readonly Assembly _assembly;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public ICivilAssemblyProperties Properties { get; }

    /// <summary>
    /// Constructs a new instance of <see cref="CivilAssembliesWrapper"/>
    /// </summary>
    public CivilAssembliesWrapper(Assembly assembly) : base(assembly)
    {
        _assembly = assembly;
        this.Name = assembly.Name;
        this.Properties = new CivilAssemblyProperties(assembly);
    }

    /// <inheritdoc />
    public List<string> GetGroupNames()
    {
        var groupNames = new List<string>();

        try
        {
            foreach (var group in _assembly.Groups)
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

    /// <inheritdoc />
    public List<ICivilSubassembly> GetSubassemblies(IAutocadTransactionManager transactionManager)
    {
        var subassemblies = new List<ICivilSubassembly>();

        var transaction = transactionManager.Unwrap();
        try
        {
            foreach (var group in _assembly.Groups)
            {
                foreach (ObjectId subassemblyId in group.GetSubassemblyIds())
                {
                    var subassembly = transaction.GetObject(subassemblyId, OpenMode.ForRead) as Subassembly;

                    var wrapper = new CivilSubassemblyWrapper(subassembly);

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
}