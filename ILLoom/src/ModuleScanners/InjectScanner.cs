using ILLoom.ModuleScanners.ScannerTypes;
using ILWrapper;
using LoomModLib.Attributes;
using CustomAttribute = ILWrapper.SubMembers.CustomAttribute;

namespace ILLoom.ModuleScanners;

public class InjectScanner : ModuleMemberScanner<object?>
{
    protected override object? ReadAttribute(CustomAttribute attribute, IMember owner)
    {
        return null;
    }
    
    protected override bool IncludeAttribute(CustomAttribute attribute)
    {
        return attribute.Type.Is<InjectAttribute>();
    }

    protected override bool RemoveTransformer(CustomAttribute attribute)
    {
        return true;
    }
}