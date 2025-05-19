using ILLoom.ModuleScanners.ScannerTypes;
using ILWrapper;
using ILWrapper.Members;
using LoomModLib.Attributes;
using Mono.Cecil;
using CustomAttribute = ILWrapper.SubMembers.CustomAttribute;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.ModuleScanners;

public class HoistScanner : ModuleMemberScanner<HoistRemapping>
{
    protected override HoistRemapping ReadAttribute(CustomAttribute attribute, IMember owner)
    {
        var targetMember = (string)attribute[0];
        var targetType = new Type((TypeReference)Program.Remap((TypeReference)attribute[1]));
        
        var target = owner switch
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
            _ => throw new Exception($"Unexpected member type: {owner.GetType()}")
        };
                
        return new HoistRemapping(owner.MemberBase.FullName, target);
    }
    
    protected override bool IncludeAttribute(CustomAttribute attribute)
    {
        var type = attribute.Type;
                    
        return type.Is<HoistAttribute>() 
               || type.Is<InjectAttribute>();
    }
    
    protected override bool RemoveTransformer(CustomAttribute attribute)
    {
        return attribute.Type.Is<HoistAttribute>();
    }
}