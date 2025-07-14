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
        var asmNameStr = attribute.Get<string>(0);
        var nameStr = attribute.Get<string>(1);
        var nsStr = attribute.Get<string>(2);
        var versionStr = attribute.Get<string>(3);
        
        var asmName = asmNameStr == "<target_asm>" ? Program.TargetModule.Assembly.Name.Name : nameStr;
        var name = nameStr == "<type_name>" ? owner.Name : nameStr;
        var ns = nsStr == "<asm_name>" ? asmName : nsStr;
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