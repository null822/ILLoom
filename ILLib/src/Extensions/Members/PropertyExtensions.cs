using ILLib.Extensions.Containers;
using Mono.Cecil;

namespace ILLib.Extensions.Members;

public static class PropertyExtensions
{
    public static PropertyDefinition Clone(this PropertyDefinition self, ParentInfo info)
    {
        var clone = new PropertyDefinition(self.Name, self.Attributes, self.PropertyType.RemapAndImport(info))
        {
            DeclaringType = info.Type?.Resolve() ?? self.DeclaringType,
            Constant = self.Constant,
            SetMethod = self.SetMethod?.Clone(info),
            GetMethod = self.GetMethod?.Clone(info),
            MetadataToken = self.MetadataToken,
        };
        info.Property = clone;
        clone.CustomAttributes.ReplaceContents(self.CustomAttributes, info);
        clone.Parameters.ReplaceContents(self.Parameters, info);
        clone.OtherMethods.ReplaceContents(self.OtherMethods, info);
        
        return clone;
    }
}