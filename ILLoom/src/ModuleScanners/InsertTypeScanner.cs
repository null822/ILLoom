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
        var assemblyName = attribute.Get<string>(0);
        var targetSignature = attribute.Get<string>(1);
        var versionStr = attribute.Get<string>(2);
        var version = versionStr == "*" ? ILLib.Util.AllVersions : Version.Parse(versionStr);
        
        var targetAssembly = Program.AssemblyResolver.Resolve(new AssemblyNameReference(assemblyName, version));
        
        return new InsertTypeTransformer(owner, targetAssembly, targetSignature);
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