using ILWrapper.MemberSet;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper.SubMembers;

public class Variable : IMember<Variable, VariableDefinition>, ISubMember
{
    public VariableDefinition Base { get; }
    static TypeConvert<VariableDefinition, Variable> IMember<Variable, VariableDefinition>.FromBase => instance => new Variable(instance);
    
    public Variable(VariableDefinition @base)
    {
        Base = @base;
    }

    public Variable(Type variableType) : this(new VariableDefinition(variableType.Base)) {}

    public string FullName => $"[{Index}] {Type.FullName}";
    
    public bool IsPinned => Base.IsPinned;
    public int Index => Base.Index;
    public Type Type { get => IMember<Type, TypeReference>.Create(Base.VariableType); set => Base.VariableType = value?.Base; }
    
    public Variable Clone(ParentInfo info)
    {
        var variable = new Variable(info.Remap(Type));
        // TODO: missing index
        return variable;
    }

    public override string ToString()
    {
        return FullName;
    }
}
