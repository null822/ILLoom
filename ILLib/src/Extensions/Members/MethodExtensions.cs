using ILLib.Extensions.Containers;
using ILLib.Extensions.SubMembers;
using Mono.Cecil;

namespace ILLib.Extensions.Members;

public static class MethodExtensions
{
    public static MethodDefinition Clone(this MethodDefinition self, ParentInfo info)
    {
        var clone = new MethodDefinition(self.Name, self.Attributes, self.ReturnType.RemapAndImport(info))
        {
            DeclaringType = info.Type?.Resolve() ?? self.DeclaringType,
            Attributes = self.Attributes,
            MetadataToken = self.MetadataToken
        };
        info.Method = clone;
        clone.CustomAttributes.ReplaceContents(self.CustomAttributes, info);
        clone.Body = self.Body.Clone(info);
        clone.Parameters.ReplaceContents(self.Parameters, info);
        clone.GenericParameters.ReplaceContents(self.GenericParameters, info);
        clone.Overrides.ReplaceContents(self.Overrides, info);
        
        return clone;
    }
    
    public static MethodReference CreateReference(this MethodReference m, string name, TypeReference targetType)
    {
        var methodRef = new MethodReference(name, m.ReturnType, targetType);
        methodRef.Parameters.Clear();
        foreach (var p in m.Parameters)
        {
            methodRef.Parameters.Add(p);
        }
        return methodRef;
    }
}