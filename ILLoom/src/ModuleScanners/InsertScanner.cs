using ILLoom.Transformers;
using ILWrapper;
using ILWrapper.Containers;
using ILWrapper.Members;
using ILWrapper.MemberSet;
using LoomModLib.Attributes;
using Mono.Cecil;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.ModuleScanners;

public class InsertScanner : IModuleScanner<List<Inserter>>
{
    private readonly List<Inserter> _inserters = [];
    
    public List<Inserter> Scan(Module module)
    {
        foreach (var type in module.Types)
        {
            if (type.FullName == "<Module>") continue;
            ScanMembers(type);
        }
        
        return _inserters;
    }
    
    private void ScanMembers(Type type)
    {
        // sort members
        Sort(CollectionConvert<IMember, Method>.Of<IMember, Method>(type.Methods));
        Sort(CollectionConvert<IMember, Field>.Of<IMember, Field>(type.Fields));
        Sort(CollectionConvert<IMember, Property>.Of<IMember, Property>(type.Properties));
        Sort(CollectionConvert<IMember, Event>.Of<IMember, Event>(type.Events));
        Sort(CollectionConvert<IMember, Type>.Of<IMember, Type>(type.NestedTypes));
    }
    
    private void Sort(IList<IMember> members)
    {
        for (var i = 0; i < members.Count; i++)
        {
            var remove = false;
            var member = members[i];

            var attribs = member.CustomAttributes
                .Where(a => a.Type.Is<InsertAttribute>());
            
            foreach (var attrib in attribs)
            {
                var newName = (string)attrib[0];
                var targetType = new Type(((TypeReference)Program.Remap((TypeReference)attrib[1])).Resolve());
                
                var inserter = new Inserter(targetType, member, newName);
                _inserters.Add(inserter);
                
                remove = true;
            }
            
            if (remove)
            {
                // Console.WriteLine(member.MemberBase.FullName);
                // this removes the member from the Module since the IList supplied is a wrapper
                members.RemoveAt(i);
                i--;
            }

        }

    }
}