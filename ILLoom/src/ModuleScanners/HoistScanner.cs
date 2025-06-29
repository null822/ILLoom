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
        var originalName = owner.MemberBase.FullName;
        
        var targetMember = attribute.Get<string>(0);
        var type = attribute.Get<TypeReference>(1);
        Type targetType;
        if (type == null!)
        {
            var ownerType = owner.MemberBase.DeclaringType;
            var remappedOwnerType = Program.Remap(ownerType);
            if (ownerType.FullName == remappedOwnerType.FullName)
                throw new InvalidHoistAttribute(owner);
            targetType = new Type((TypeReference)remappedOwnerType);
        }
        else
        {
            targetType = new Type((TypeReference)Program.Remap(type));
        }
        
        MemberReference target = owner switch
        {
            Method m => CreateMethodReference(targetMember, m, targetType),
            Field m => new FieldReference(targetMember, m.FieldType.Base, targetType.Base),
            Property m => new PropertyDefinition(targetMember, m.Attributes, targetType.Base),
            Event m => new EventDefinition(targetMember, m.Attributes, targetType.Base),
            _ => throw new Exception($"Unexpected member type: {owner.GetType()}")
        };
        
        return new HoistRemapping(originalName, target);
    }

    private static MethodReference CreateMethodReference(string name, Method m, Type targetType)
    {
        var methodRef = new MethodReference(name, m.ReturnType.Base, targetType.Base);
        methodRef.Parameters.Clear();
        foreach (var p in m.Parameters)
        {
            methodRef.Parameters.Add(p.Base);
        }
        return methodRef;
    }
    
    protected override bool IncludeAttribute(CustomAttribute attribute)
    {
        var type = attribute.Type;
                    
        return type.Is<HoistAttribute>() 
               || type.Is<InjectAttribute>()
               || type.Is<InsertAttribute>();
    }
    
    protected override bool RemoveTransformer(CustomAttribute attribute)
    {
        return attribute.Type.Is<HoistAttribute>();
    }
}

public class InvalidHoistAttribute(IMember owner)
    : Exception($"[Hoist] attribute on '{owner.MemberBase.FullName}' without `targetType` parameter must be in a class with a [HoistType] attribute");
