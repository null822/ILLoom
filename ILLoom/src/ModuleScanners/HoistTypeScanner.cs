using ILLoom.ModuleScanners.ScannerTypes;
using ILLib.Extensions.Containers;
using ILLib.Extensions.SubMembers;
using LoomModLib.Attributes;
using Mono.Cecil;

namespace ILLoom.ModuleScanners;

public class HoistTypeScanner : ModuleClassScanner<HoistRemapping>
{
    protected override HoistRemapping ReadAttribute(CustomAttribute attribute, TypeDefinition owner)
    {
        var target = Util.CreateTypeReference(
            attribute.Get<string>(0),
            Version.Parse(attribute.Get<string>(1)),
            attribute.Get<string>(2));
            
        return new HoistRemapping(owner.FullName, target);
    }
    
    protected override bool IncludeAttribute(CustomAttribute attribute)
    {
        var attribType = attribute.AttributeType;
        return attribType.Is<HoistTypeAttribute>() || attribType.Is<InsertTypeAttribute>();
    }
    
    protected override bool RemoveTransformer(CustomAttribute attribute)
    {
        return attribute.AttributeType.Is<HoistTypeAttribute>();
    }
}