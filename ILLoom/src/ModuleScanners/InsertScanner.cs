using ILLoom.ModuleScanners.ScannerTypes;
using ILLoom.Transformers;
using ILLib.Extensions.Containers;
using ILLib.Extensions.SubMembers;
using LoomModLib.Attributes;
using Mono.Cecil;

namespace ILLoom.ModuleScanners;

public class InsertScanner : ModuleMemberScanner<InsertTransformer>
{
    protected override InsertTransformer ReadAttribute(CustomAttribute attribute, IMemberDefinition owner)
    {
        var newName = attribute.Get<string>(0);
        var targetType = Program.Remap(attribute.Get<TypeReference>(1)).Resolve();
        
        return new InsertTransformer(targetType, owner, newName);
    }

    protected override bool IncludeAttribute(CustomAttribute attribute)
    {
        return attribute.AttributeType.Is<InsertAttribute>();
    }

    protected override bool RemoveTransformer(CustomAttribute attribute)
    {
        return true;
    }
}