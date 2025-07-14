using ILLib.Extensions.Containers;
using Mono.Cecil;

namespace ILLib.Extensions.Members;

public static class FieldExtensions
{
    public static FieldDefinition Clone(this FieldDefinition self, ParentInfo info)
    {
        var clone = new FieldDefinition(self.Name, self.Attributes, self.FieldType.RemapAndImport(info))
        {
            DeclaringType = info.Type?.Resolve() ?? self.DeclaringType,
            Constant = self.Constant,
            InitialValue = self.InitialValue,
            Offset = self.Offset,
            MetadataToken = self.MetadataToken,
            MarshalInfo = self.MarshalInfo
        };
        info.Field = clone;
        clone.CustomAttributes.ReplaceContents(self.CustomAttributes, info);
        
        return clone;
    }
}