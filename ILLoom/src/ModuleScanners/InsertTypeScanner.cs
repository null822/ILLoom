using ILWrapper.Containers;
using LoomModLib.Attributes;
using Mono.Cecil;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.ModuleScanners;

public class InsertTypeScanner : IModuleScanner<object?>
{
    public object? Scan(Module module)
    {
        for (var i = 0; i < module.Types.Count; i++)
        {
            var type = module.Types[i];
            if (type.FullName == "<Module>") continue;
            if (ScanType(type, module))
            {
                type.NestedTypes.RemoveAt(i);
                i--;
            }
        }

        return null;
    }
    
    private bool ScanType(Type type, Module module)
    {
        var remove = false;

        var hoistTypeAttribs = type.CustomAttributes
            .Where(a => a.Type.Is<InsertTypeAttribute>());
        
        foreach (var attrib in hoistTypeAttribs)
        {
            var newName = (string)attrib[0];
            var parentType = (TypeReference)attrib[1];
            
            remove = true;
        }
        
        
        // scan all nested types
        for (var i = 0; i < type.NestedTypes.Count; i++)
        {
            var nestedType = type.NestedTypes[i];
            if (ScanType(nestedType, module))
            {
                type.NestedTypes.RemoveAt(i);
                i--;
            }
        }

        return remove;
    }
}