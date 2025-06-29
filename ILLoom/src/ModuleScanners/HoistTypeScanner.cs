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
            attribute.Get<string>(0),
            Version.Parse(attribute.Get<string>(1)),
            attribute.Get<string>(2));
            
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