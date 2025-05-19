using ILLoom.Transformers.TransformerTypes;
using ILWrapper;
using ILWrapper.Members;
using LoomModLib.Attributes;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.Transformers;

public class InsertTransformer(Type type, IMember member, string newName) : ITransformer
{
    public string Name => $"{member.MemberBase.FullName} => {type.FullName} as {newName}";

    public void Apply()
    {
        var info = type.Info;
        info.Remapper = Program.Remapper;
        
        switch (member)
        {
            case Field m:
                type.Fields.Add(PrepareMember(m.Clone(info)));
                break;
            case Method m:
                type.Methods.Add(PrepareMember(m.Clone(info)));
                break;
            case Property m:
                type.Properties.Add(PrepareMember(m.Clone(info)));
                break;
            case Event m:
                type.Events.Add(PrepareMember(m.Clone(info)));
                break;
            case Type m:
                type.NestedTypes.Add(PrepareMember(m.Clone(info)));
                break;
        }
    }
    
    private T PrepareMember<T>(T m) where T : IMember
    {
        m.MemberBase.Name = newName;
        m.MemberBase.DeclaringType = null!;
        m.CustomAttributes.RemoveAll(a => a.Type.Is<DontCopyAttribute>());
        return m;
    }
}