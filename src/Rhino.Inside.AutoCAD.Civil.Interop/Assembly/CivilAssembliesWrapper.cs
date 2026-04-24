using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Core.Interfaces.Assemblies;
using Rhino.Inside.AutoCAD.Interop;
using CivilSubassembly = Autodesk.Civil.DatabaseServices.Subassembly;
using RhinoPoint3d = Rhino.Geometry.Point3d;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

public class CivilAssembliesWrapper : AutocadEntityWrapper, ICivilAssemblies
{
    private readonly Assembly _assembly;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public CivilAssemblyType AssemblyType { get; }

    /// <inheritdoc />
    public string Code { get; }

    /// <inheritdoc />
    public INamedId Style { get; }

    /// <inheritdoc />
    public IObjectId AssemblyId => new AutocadObjectIdWrapper(_assembly.Id);

    /// <inheritdoc />
    public RhinoPoint3d Location { get; }

    /// <summary>
    /// Constructs a new instance of <see cref="CivilAssembliesWrapper"/>
    /// </summary>
    public CivilAssembliesWrapper(Assembly assembly) : base(assembly)
    {
        _assembly = assembly;
        this.Name = assembly.Name;
        this.Description = assembly.Description ?? string.Empty;
        this.AssemblyType = assembly.Type.ToRhinoInsideAssemblyType();
        this.Code = assembly.CodeSetStyleName ?? string.Empty;
        this.Style = new NamedId(assembly.StyleName, assembly.StyleId);
        this.Location = assembly.Location.ToRhinoPoint3d();
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
                    var subassembly = transaction.GetObject(subassemblyId, OpenMode.ForRead) as CivilSubassembly;

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

    /// <inheritdoc />
    public ICivilAssemblies Update(IAutocadTransactionManager transactionManager,
        string newName, string newDescription, CivilAssemblyType newType,
        string newCode, IObjectId newStyleId, RhinoPoint3d newLocation)
    {
        var assembly = transactionManager.Unwrap().GetObject(_assembly.Id, OpenMode.ForWrite) as Assembly;

        if (assembly == null)
        {
            return this;
        }

        assembly.Name = newName;
        assembly.Description = newDescription;
        assembly.Type = newType.ToCivilAssemblyType();
        assembly.CodeSetStyleName = newCode;
        assembly.StyleId = newStyleId.Unwrap();
        assembly.Location = newLocation.ToAutocadPoint3d();

        return new CivilAssembliesWrapper(assembly);
    }
}