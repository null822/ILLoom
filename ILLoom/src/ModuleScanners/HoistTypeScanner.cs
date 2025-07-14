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
        var versionStr = attribute.Get<string>(2);
        var version = versionStr == "*" ? ILLib.Util.AllVersions : Version.Parse(versionStr);
        
        var target = Util.CreateTypeReference(
            attribute.Get<string>(0),
            version,
            attribute.Get<string>(1));
            
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