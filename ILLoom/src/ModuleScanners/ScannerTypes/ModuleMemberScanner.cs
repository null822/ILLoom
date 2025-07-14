using Mono.Cecil;
using Mono.Collections.Generic;

namespace ILLoom.ModuleScanners.ScannerTypes;

public abstract class ModuleMemberScanner<T> : IModuleScanner<T>
{
    private readonly List<T> _results = [];
    
    /// <summary>
    /// Reads a <see cref="CustomAttribute"/> and returns a new <typeparamref name="T"/>
    /// </summary>
    /// <param name="attribute">the attribute to scan</param>
    /// <param name="owner">the <see cref="IMember"/> that has the attribute</param>
    protected abstract T ReadAttribute(CustomAttribute attribute, IMemberDefinition owner);
    /// <summary>
    /// Returns whether to read an attribute (true), or ignore it (false)
    /// </summary>
    /// <param name="attribute">the attribute</param>
    protected abstract bool IncludeAttribute(CustomAttribute attribute);
    /// <summary>
    /// Returns whether to replace this attribute with a [DontCopy] attribute, preventing the member from being copied
    /// into the target executable
    /// </summary>
    /// <param name="attribute">the attribute</param>
    protected abstract bool RemoveTransformer(CustomAttribute attribute);
    
    public List<T> Scan(ModuleDefinition module)
    {
        foreach (var type in module.Types)
        {
            if (type.FullName == "<Module>") continue;
            Scan(type);
        }
        
        return _results;
    }
    
    private void Scan(TypeDefinition type)
    {
        Scan(type.Methods);
        Scan(type.Fields);
        Scan(type.Properties);
        Scan(type.Events);
        Scan(type.NestedTypes);
    }
    
    private void Scan<T2>(Collection<T2> members) where T2 : IMemberDefinition
    {
        foreach (var member in members)
        {
            Scan(member);
        }
    }
    
    private void Scan(IMemberDefinition member)
    {
        for (var i = 0; i < member.CustomAttributes.Count; i++)
        {
            var attrib = member.CustomAttributes[i];
                
            // skip irrelevant attributes
            if (!IncludeAttribute(attrib))
                continue;
            
            // read the attribute
            _results.Add(ReadAttribute(attrib, member));

            // replace the attribute with a [DontCopy] attribute if needed
            if (RemoveTransformer(attrib))
            {
                member.CustomAttributes[i] = Util.GetDontCopyAttribute();
            }
        }

        if (member is TypeDefinition t)
        {
            Scan(t);
        }
    }
}