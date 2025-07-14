using ILLib.Extensions.Containers;
using Mono.Cecil.Cil;

namespace ILLib.Extensions.SubMembers;

public static class VariableExtensions
{
    public static VariableDefinition Clone(this VariableDefinition self, ParentInfo info)
    {
        var clone = new VariableDefinition(self.VariableType.RemapAndImport(info));
        return clone;
    }
}