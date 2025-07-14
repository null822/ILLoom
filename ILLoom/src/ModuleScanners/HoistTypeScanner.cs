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
        var asmName = attribute.Get<string>(0);
        var name = attribute.Get<string>(1);
        var nsString = attribute.Get<string>(2);
        var versionStr = attribute.Get<string>(3);
        
        var ns = nsString == "<asm_name>" ? asmName : nsString;
        var version = versionStr == "*" ? ILLib.Util.AllVersions : Version.Parse(versionStr);
        
        var target = Util.CreateTypeReference(asmName, name, version, ns);
        
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