using ILWrapper;
using ILWrapper.Containers;
using ILWrapper.Members;
using LoomModLib.Attributes;
using Mono.Cecil;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.ModuleScanners;

public class HoistScanner : IModuleScanner<List<(string, MemberReference)>>
{
    private readonly List<(string, MemberReference)> _hoists = [];
    
    public List<(string, MemberReference)> Scan(Module module)
    {
        foreach (var type in module.Types)
        {
            if (type.FullName == "<Module>") continue;
            ScanMembers(type);
        }
        
        return _hoists;
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
                .Where(a =>
                {
                    var type = a.Type;
                    
                    return type.Is<InjectEnumAttribute>()
                           || type.Is<InjectAttribute>()
                           || type.Is<HoistAttribute>();
                });
            
            
            // TODO:
            // Hoist on methods/fields/(etc. probably) seems not to work
            // Insert doesn't either
            // Constructors are left invalid after members are removed
            
            foreach (var attrib in attribs)
            {
                var targetMember = (string)attrib[0];
                var targetType = new Type((TypeReference)Program.Remap((TypeReference)attrib[1]));
                
                var target = member switch
                {
                    Method method => (MemberReference)
                        targetType.Methods
                            .Where(m => m.Name == targetMember)
                            .First(m => m.Parameters.Matches(method.Parameters))
                            .Base,
                    Field => targetType.Fields.First(m => m.Name == targetMember).Base,
                    Property => targetType.Properties.First(m => m.Name == targetMember).Base,
                    Event => targetType.Events.First(m => m.Name == targetMember).Base,
                    Type => targetType.NestedTypes.First(m => m.Name == targetMember).Base,
                    _ => throw new Exception($"Unexpected member type: {member.GetType()}")
                };
                
                _hoists.Add((member.MemberBase.FullName, target));
                remove = true;
            }

            if (remove)
            {
                // this removes the member from the Module since the IList supplied is a wrapper
                members.RemoveAt(i);
                i--;
            }

        }

    }
}