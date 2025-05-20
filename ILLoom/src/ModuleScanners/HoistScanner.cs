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
        var targetMember = (string)attribute[0]!;
        var type = attribute[1];
        Type targetType;
        if (type == null)
        {
            var ownerType = owner.MemberBase.DeclaringType;
            var remappedOwnerType = Program.Remap(ownerType);
            if (ownerType.FullName == remappedOwnerType.FullName)
                throw new InvalidHoistAttribute(owner);
            targetType = new Type((TypeReference)remappedOwnerType);
        }
        else
        {
            targetType = new Type((TypeReference)Program.Remap((TypeReference)type));
        }

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

public class InvalidHoistAttribute(IMember owner)
    : Exception($"[Hoist] attribute on '{owner.MemberBase.FullName}' without `targetType` parameter must be in a class with a [HoistType] attribute");
