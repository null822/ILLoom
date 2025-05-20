using ILLoom.ModuleScanners.ScannerTypes;
using ILLoom.Transformers;
using LoomModLib.Attributes;
using Mono.Cecil;
using CustomAttribute = ILWrapper.SubMembers.CustomAttribute;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.ModuleScanners;

public class InjectEnumScanner : ModuleClassScanner<InjectEnumTransformer>
{
    protected override InjectEnumTransformer ReadAttribute(CustomAttribute attribute, Type owner)
    {
        var target = new Type((TypeReference)Program.Remap((TypeReference)attribute[0]!));
        return new InjectEnumTransformer(owner, target);
    }

    protected override bool IncludeAttribute(CustomAttribute attribute)
    {
        return attribute.Type.Is<InjectEnumAttribute>();
    }

    protected override bool RemoveTransformer(CustomAttribute attribute)
    {
        return true;
    }
}