using ILLoom.ModuleScanners.ScannerTypes;
using ILLoom.Transformers;
using ILWrapper.Containers;
using LoomModLib.Attributes;
using Mono.Cecil;
using CustomAttribute = ILWrapper.SubMembers.CustomAttribute;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.ModuleScanners;

public class InsertTypeScanner : ModuleClassScanner<InsertTypeTransformer>
{
    protected override InsertTypeTransformer ReadAttribute(CustomAttribute attribute, Type owner)
    {
        var assemblyName = attribute.Get<string>(0);
        var assemblyVersion = Version.Parse(attribute.Get<string>(1));
        var targetSignature = attribute.Get<string>(2);
            
        var targetAssembly = Program.AssemblyResolver.Resolve(new AssemblyNameReference(assemblyName, assemblyVersion));
        
        return new InsertTypeTransformer(owner, new Assembly(targetAssembly), targetSignature);
    }
    
    protected override bool IncludeAttribute(CustomAttribute attribute)
    {
        return attribute.Type.Is<InsertTypeAttribute>();
    }
    
    protected override bool RemoveTransformer(CustomAttribute attribute)
    {
        return true;
    }
}