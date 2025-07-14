using ILLoom.Transformers.TransformerTypes;
using ILLib.Extensions;
using ILLib.Extensions.Containers;
using ILLib.Extensions.Members;
using LoomModLib.Attributes;
using Mono.Cecil;

namespace ILLoom.Transformers;

public class InsertTransformer(TypeDefinition type, IMemberDefinition member, string newName) : ITransformer
{
    public string Name => $"{member.FullName} => {type.FullName} as {newName}";

    public void Apply()
    {
        var info = Program.TargetInfo.With(type);
        
        switch (member)
        {
            case FieldDefinition m:
                type.Fields.Add(PrepareMember(m.Clone(info)));
                break;
            case MethodDefinition m:
                type.Methods.Add(PrepareMember(m.Clone(info)));
                break;
            case PropertyDefinition m:
                type.Properties.Add(PrepareMember(m.Clone(info)));
                break;
            case EventDefinition m:
                type.Events.Add(PrepareMember(m.Clone(info)));
                break;
            case TypeDefinition m:
                type.NestedTypes.Add(PrepareMember(m.Clone(info)));
                break;
        }
    }
    
    private T PrepareMember<T>(T m) where T : IMemberDefinition
    {
        m.Name = newName;
        m.DeclaringType = null!;
        m.CustomAttributes.RemoveAll(a => a.AttributeType.Is<DontCopyAttribute>());
        return m;
    }
}