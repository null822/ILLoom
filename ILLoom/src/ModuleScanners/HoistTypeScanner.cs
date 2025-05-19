using ILLoom.ModuleScanners.ScannerTypes;
using LoomModLib.Attributes;
using CustomAttribute = ILWrapper.SubMembers.CustomAttribute;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.ModuleScanners;

public class HoistTypeScanner : ModuleClassScanner<HoistRemapping>
{
    protected override HoistRemapping ReadAttribute(CustomAttribute attribute, Type owner)
    {
        var target = Util.CreateTypeReference(
            (string)attribute[0],
            Version.Parse((string)attribute[1]),
            (string)attribute[2]);
            
        return new HoistRemapping(owner.FullName, target);
    }
    
    protected override bool IncludeAttribute(CustomAttribute attribute)
    {
        var attribType = attribute.Type;
        return attribType.Is<HoistTypeAttribute>() || attribType.Is<InsertTypeAttribute>();
    }
    
    protected override bool RemoveTransformer(CustomAttribute attribute)
    {
        return attribute.Type.Is<HoistTypeAttribute>();
    }
}