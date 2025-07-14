using ILLoom.ModuleScanners.ScannerTypes;
using ILLoom.Transformers;
using ILLib.Extensions.Containers;
using ILLib.Extensions.SubMembers;
using LoomModLib.Attributes;
using Mono.Cecil;

namespace ILLoom.ModuleScanners;

public class InsertTypeScanner : ModuleClassScanner<InsertTypeTransformer>
{
    protected override InsertTypeTransformer ReadAttribute(CustomAttribute attribute, TypeDefinition owner)
    {
        var asmName = attribute.Get<string>(0);
        var nameStr = attribute.Get<string>(1);
        var nsStr = attribute.Get<string>(2);
        var versionStr = attribute.Get<string>(3);
        
        var name = nameStr == "<type_name>" ? owner.Name : nameStr;
        var ns = nsStr == "<asm_name>" ? asmName : nsStr;
        var version = versionStr == "*" ? ILLib.Util.AllVersions : Version.Parse(versionStr);
        
        var targetAssembly = Program.AssemblyResolver.Resolve(new AssemblyNameReference(asmName, version));
        
        return new InsertTypeTransformer(owner, targetAssembly, name, ns);
    }
    
    protected override bool IncludeAttribute(CustomAttribute attribute)
    {
        return attribute.AttributeType.Is<InsertTypeAttribute>();
    }
    
    protected override bool RemoveTransformer(CustomAttribute attribute)
    {
        return true;
    }
}