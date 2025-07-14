using ILLib.Extensions.Containers;
using Mono.Cecil;

namespace ILLib.Extensions.SubMembers;

public static class ParameterExtensions
{
    public static ParameterDefinition Clone(this ParameterDefinition self, ParentInfo info)
    {
        var clone = new ParameterDefinition(self.Name, self.Attributes, self.ParameterType.RemapAndImport(info))
        {
            Attributes = self.Attributes,
            MarshalInfo = self.MarshalInfo,
            Constant = self.Constant,
            HasDefault = self.HasDefault,
            HasConstant = self.HasConstant,
            HasFieldMarshal = self.HasFieldMarshal,
            IsIn = self.IsIn,
            IsOut = self.IsOut,
            IsLcid = self.IsLcid,
            IsOptional = self.IsOptional,
            IsReturnValue = self.IsReturnValue
        };
        clone.CustomAttributes.ReplaceContents(self.CustomAttributes, info);
        return clone;
    }
}