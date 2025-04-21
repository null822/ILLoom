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
                    || t.Is<InsertTypeAttribute>();
            });
        
        foreach (var attrib in hoistTypeAttribs)
        {
            var assemblyName = (string)attrib[0];
            var version = Version.Parse((string)attrib[1]);
            var @namespace = (string)attrib[2];
            var targetSignature = (string)attrib[3];
            
            var path = targetSignature.Split('.');
            
            var targetAssembly = Program.AssemblyResolver.Resolve(new AssemblyNameReference(assemblyName, version));
            var targetRef = new TypeReference(@namespace, path[0], module.Base, targetAssembly.MainModule);
            
            var target = targetRef.Resolve();
            
            for (var i = 1; i < path.Length; i++)
            {
                target = target.NestedTypes.First(t => t.Name == path[i]);
            }
            
            _hoists.Add((type.Base.FullName, target));
            
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