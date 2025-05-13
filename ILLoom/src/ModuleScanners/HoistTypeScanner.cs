using ILWrapper.Containers;
using LoomModLib.Attributes;
using Mono.Cecil;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.ModuleScanners;

public class HoistTypeScanner : IModuleScanner<List<(string, MemberReference)>>
{
    private readonly List<(string, MemberReference)> _hoists = [];
    
    public List<(string, MemberReference)> Scan(Module module)
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

        return _hoists;
    }
    
    private bool ScanType(Type type, Module module)
    {
        var remove = false;

        var hoistTypeAttribs = type.CustomAttributes
            .Where(a =>
            {
                var t = a.Type;
                
                return t.Is<HoistTypeAttribute>()
                     || t.Is<InsertTypeAttribute>(); // TODO: enable once InsertTypeAttribute is fully implemented
            });
        
        foreach (var attrib in hoistTypeAttribs)
        {
            var target = Util.CreateTypeReference(
                (string)attrib[0],
                Version.Parse((string)attrib[1]),
                (string)attrib[2]);
            
            _hoists.Add((type.Base.FullName, target));
            
            // remove all explicitly [Hoist]ed types
            if (attrib.Type.Is<HoistTypeAttribute>())
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