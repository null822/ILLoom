using ILWrapper;
using ILWrapper.Containers;
using ILWrapper.MemberSet;
using ILWrapper.SubMembers;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.ModuleScanners.ScannerTypes;

public abstract class ModuleMemberScanner<T> : IModuleScanner<T>
{
    private readonly List<T> _results = [];
    
    protected abstract T ReadAttribute(CustomAttribute attribute, IMember owner);
    protected abstract bool IncludeAttribute(CustomAttribute attribute);
    protected abstract bool RemoveTransformer(CustomAttribute attribute);
    
    public List<T> Scan(Module module)
    {
        foreach (var type in module.Types)
        {
            if (type.FullName == "<Module>") continue;
            Scan(type);
        }
        
        return _results;
    }
    
    private void Scan(Type type)
    {
        Scan(type.Methods);
        Scan(type.Fields);
        Scan(type.Properties);
        Scan(type.Events);
        Scan(type.NestedTypes);
    }
    
    private void Scan<T2>(IMemberSet<T2> members) where T2 : IMember
    {
        foreach (var member in members)
        {
            Scan(member);
        }
    }
    
    private void Scan(IMember member)
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

        if (member is Type t)
        {
            Scan(t);
        }
    }
}