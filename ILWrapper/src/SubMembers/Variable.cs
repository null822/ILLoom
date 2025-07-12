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
    public Type Type { get => new(Base.VariableType); set => Base.VariableType = value.Base; }
    
    public Variable Clone(ParentInfo info)
    {
        var type = Type;
        
        if (info.Module != null)
            type = info.Remap(type);
        if (info is { RuntimeAssembly: not null, Module: not null, Module.MetadataResolver: not null })
            type.TryChangeAssembly(info.RuntimeAssembly, info.Module.MetadataResolver, out type);
        if (info.Module != null)
            type = info.Module.TryImportReference(type);
        
        var variable = new Variable(type);
        
        return variable;
    }

    public override string ToString()
    {
        return FullName;
    }
}
