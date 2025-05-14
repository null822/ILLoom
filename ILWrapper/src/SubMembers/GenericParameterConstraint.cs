using ILWrapper.MemberSet;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper.SubMembers;

public class GenericParameterConstraint : IMember<GenericParameterConstraint, Mono.Cecil.GenericParameterConstraint>, ISubMember
{
    public Mono.Cecil.GenericParameterConstraint Base { get; }
    static TypeConvert<Mono.Cecil.GenericParameterConstraint, GenericParameterConstraint> IMember<GenericParameterConstraint, Mono.Cecil.GenericParameterConstraint>.FromBase => instance => new GenericParameterConstraint(instance);
    
    public GenericParameterConstraint(Mono.Cecil.GenericParameterConstraint @base)
    {
        Base = @base;
        
        Attributes = new MemberSet<CustomAttribute, Mono.Cecil.CustomAttribute>(Base.CustomAttributes);
    }
    
    public GenericParameterConstraint(Type constraintType) : this(new Mono.Cecil.GenericParameterConstraint(constraintType.Base)) {}
    
    public string FullName => $"<{ConstraintType.FullName}>";
    
    public Type ConstraintType { get => new(Base.ConstraintType); set => Base.ConstraintType = value.Base; }
    
    public readonly IMemberSet<CustomAttribute> Attributes;
    
    public GenericParameterConstraint Clone(ParentInfo info)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return FullName;
    }
}
