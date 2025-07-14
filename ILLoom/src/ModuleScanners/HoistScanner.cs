using ILLoom.ModuleScanners.ScannerTypes;
using ILLib.Extensions.Containers;
using ILLib.Extensions.Members;
using ILLib.Extensions.SubMembers;
using LoomModLib.Attributes;
using Mono.Cecil;

namespace ILLoom.ModuleScanners;

public class HoistScanner : ModuleMemberScanner<HoistRemapping>
{
    protected override HoistRemapping ReadAttribute(CustomAttribute attribute, IMemberDefinition owner)
    {
        var originalName = owner.FullName;
        
        var targetMember = attribute.Get<string>(0);
        var type = attribute.Get<TypeReference>(1);
        TypeReference targetType;
        if (type == null!)
        {
            var ownerType = owner.DeclaringType;
            var remappedOwnerType = Program.Remap(ownerType);
            if (ownerType.FullName == remappedOwnerType.FullName)
                throw new InvalidHoistAttribute(owner);
            targetType = remappedOwnerType;
        }
        else
        {
            targetType = Program.Remap(type);
        }
        
        MemberReference target = owner switch
        {
            MethodDefinition m => m.CreateReference(targetMember, targetType),
            FieldDefinition m => new FieldReference(targetMember, m.FieldType, targetType),
            PropertyDefinition m => new PropertyDefinition(targetMember, m.Attributes, targetType),
            EventDefinition m => new EventDefinition(targetMember, m.Attributes, targetType),
            _ => throw new Exception($"Unexpected member type: {owner.GetType()}")
        };
        
        return new HoistRemapping(originalName, target);
    }
    
    protected override bool IncludeAttribute(CustomAttribute attribute)
    {
        var type = attribute.AttributeType;
                    
        return type.Is<HoistAttribute>() 
               || type.Is<InjectAttribute>()
               || type.Is<InsertAttribute>();
    }
    
    protected override bool RemoveTransformer(CustomAttribute attribute)
    {
        return attribute.AttributeType.Is<HoistAttribute>();
    }
}

public class InvalidHoistAttribute(IMemberDefinition owner)
    : Exception($"[Hoist] attribute on '{owner.FullName}' without `targetType` parameter must be in a class with a [HoistType] attribute");
