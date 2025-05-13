using ILLoom.Transformers;
using ILWrapper.Containers;
using LoomModLib.Attributes;
using Mono.Cecil;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.ModuleScanners;

public class InsertTypeScanner : IModuleScanner<List<TypeInserter>>
{
    private readonly List<TypeInserter> _inserters = [];
    
    public List<TypeInserter> Scan(Module module)
    {
        for (var i = 0; i < module.Types.Count; i++)
        {
            var type = module.Types[i];
            if (type.FullName == "<Module>") continue;
            ScanType(type, module); // the types are removed in HoistTypeScanner, not here
        }

        return _inserters;
    }
    
    private void ScanType(Type type, Module module)
    {
        var hoistTypeAttribs = type.CustomAttributes
            .Where(a => a.Type.Is<InsertTypeAttribute>());
        
        foreach (var attrib in hoistTypeAttribs)
        {
            var assemblyName = (string)attrib[0];
            var assemblyVersion = Version.Parse((string)attrib[1]);
            var targetSignature = (string)attrib[2];
            
            var targetAssembly = Program.AssemblyResolver.Resolve(new AssemblyNameReference(assemblyName, assemblyVersion));
            
            var inserter = new TypeInserter(type, new Assembly(targetAssembly), targetSignature);
            _inserters.Add(inserter);
        }
        
        
        // scan all nested types
        foreach (var nestedType in type.NestedTypes)
        {
            ScanType(nestedType, module);
        }
    }
}