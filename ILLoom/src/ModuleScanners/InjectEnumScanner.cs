using ILLoom.ModuleScanners.ScannerTypes;
using ILLoom.Transformers;
using ILLib.Extensions.Containers;
using ILLib.Extensions.SubMembers;
using LoomModLib.Attributes;
using Mono.Cecil;

namespace ILLoom.ModuleScanners;

public class InjectEnumScanner : ModuleClassScanner<InjectEnumTransformer>
{
    protected override InjectEnumTransformer ReadAttribute(CustomAttribute attribute, TypeDefinition owner)
    {
        var target = Program.Remap(attribute.Get<TypeReference>(0)).Resolve();
        return new InjectEnumTransformer(owner, target);
    }

    protected override bool IncludeAttribute(CustomAttribute attribute)
    {
        return attribute.AttributeType.Is<InjectEnumAttribute>();
    }
    
    protected override bool RemoveTransformer(CustomAttribute attribute)
    {
        return true;
    }
}