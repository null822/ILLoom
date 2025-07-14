using ILLib.Extensions.Containers;
using Mono.Cecil;

namespace ILLib.Extensions.SubMembers;

public static class InterfaceImplementationExtensions
{
    public static InterfaceImplementation Clone(this InterfaceImplementation self, ParentInfo info)
    {
        info.RequireTypes(ParentInfoType.Type);
        
        var clone = new InterfaceImplementation(self.InterfaceType.RemapAndImport(info));
        clone.CustomAttributes.ReplaceContents(self.CustomAttributes, info);
        
        return clone;
    }
}