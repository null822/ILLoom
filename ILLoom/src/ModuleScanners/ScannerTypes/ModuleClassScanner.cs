using Mono.Cecil;

namespace ILLoom.ModuleScanners.ScannerTypes;

public abstract class ModuleClassScanner<T> : IModuleScanner<T>
{
    private readonly List<T> _results = [];
    
    protected abstract T ReadAttribute(CustomAttribute attribute, TypeDefinition owner);
    protected abstract bool IncludeAttribute(CustomAttribute attribute);
    protected abstract bool RemoveTransformer(CustomAttribute attribute);
    
    public List<T> Scan(ModuleDefinition module)
    {
        foreach (var type in module.Types)
        {
            if (type.FullName == "<Module>") continue;
            ScanType(type);
        }

        return _results;
    }
    
    private void ScanType(TypeDefinition type)
    {
        for (var i = 0; i < type.CustomAttributes.Count; i++)
        {
            var attrib = type.CustomAttributes[i];
            
            // skip irrelevant attributes
            if (!IncludeAttribute(attrib))
                continue;
            
            // read the attribute
            _results.Add(ReadAttribute(attrib, type));
            
            // replace the attribute with a [DontCopy] attribute if needed
            if (RemoveTransformer(attrib))
            {
                type.CustomAttributes[i] = Util.GetDontCopyAttribute();
            }
        }
        
        // scan all nested types
        foreach (var nestedType in type.NestedTypes)
        {
            ScanType(nestedType);
        }
    }
}