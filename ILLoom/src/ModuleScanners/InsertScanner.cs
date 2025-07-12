using ILLoom.ModuleScanners.ScannerTypes;
using ILLoom.Transformers;
using ILWrapper;
using LoomModLib.Attributes;
using Mono.Cecil;
using CustomAttribute = ILWrapper.SubMembers.CustomAttribute;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.ModuleScanners;

public class InsertScanner : ModuleMemberScanner<InsertTransformer>
{
    protected override InsertTransformer ReadAttribute(CustomAttribute attribute, IMember owner)
    {
        var newName = attribute.Get<string>(0);
        var targetType = new Type(
            Program.Remap(attribute.Get<TypeReference>(1)).Resolve());
        
        return new InsertTransformer(targetType, owner, newName);
    }

    protected override bool IncludeAttribute(CustomAttribute attribute)
    {
        return attribute.Type.Is<InsertAttribute>();
    }

    protected override bool RemoveTransformer(CustomAttribute attribute)
    {
        return true;
    }
}