using ILLib.Extensions.Containers;
using Mono.Cecil;

namespace ILLib.Extensions.SubMembers;

public static class CustomAttributeExtensions
{
    public static T Get<T>(this CustomAttribute self, int i) => (T)self.ConstructorArguments[i].Value;
    
    public static T As<T>(this CustomAttributeArgument self) => (T)self.Value;
    
    public static CustomAttribute Clone(this CustomAttribute self, ParentInfo info)
    {
        info.RequireTypes(ParentInfoType.Module);

        var ctor = self.Constructor.Resolve(); // CustomAttribute's ctor breaks if the ctor is not resolved
        ctor.DeclaringType = ctor.DeclaringType.RemapAndImport(info, out var error).Resolve();
        if (error)
        {
            Util.Log($"Skipping {nameof(CustomAttribute)} {ctor.DeclaringType.FullName}({ctor.Parameters.ContentsToString(p => p.ParameterType.Name)})");
            return null!;
        }
        
        var blob = Array.Empty<byte>();
        try
        {
            blob = self.GetBlob();
        }
        catch (NotSupportedException) {}
        
        var clone = new CustomAttribute(ctor, blob);
        clone.ConstructorArguments.ReplaceContents(self.ConstructorArguments, info);
        clone.Properties.ReplaceContents(self.Properties, info);
        clone.Fields.ReplaceContents(self.Fields, info);
        
        return clone;
    }
    
    public static CustomAttributeArgument Clone(this CustomAttributeArgument self, ParentInfo info)
    {
        var clone = new CustomAttributeArgument(self.Type.RemapAndImport(info), self.Value);
        return clone;
    }
    
    public static CustomAttributeNamedArgument Clone(this CustomAttributeNamedArgument self, ParentInfo info)
    {
        var clone = new CustomAttributeNamedArgument(self.Name, self.Argument);
        return clone;
    }
}