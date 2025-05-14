
using ILWrapper.MemberSet;
using Mono.Cecil;

namespace ILWrapper.SubMembers;

public class GenericParameter : IMember<GenericParameter, Mono.Cecil.GenericParameter>, ISubMember
{
    public Mono.Cecil.GenericParameter Base { get; }
    static TypeConvert<Mono.Cecil.GenericParameter, GenericParameter> IMember<GenericParameter, Mono.Cecil.GenericParameter>.FromBase => instance => new GenericParameter(instance);
    
    public GenericParameter(Mono.Cecil.GenericParameter @base)
    {
        Base = @base;
        
        Constraints = new MemberSet<GenericParameterConstraint, Mono.Cecil.GenericParameterConstraint>(Base.Constraints);
    }

    public GenericParameter(string name, IGenericParameterProvider owner) : this(new Mono.Cecil.GenericParameter(name, owner)) {}
    
    public string Name { get => Base.Name; set => Base.Name = value; }
    public string FullName => Base.FullName;
    
    public GenericParameterType Type => Base.Type;
    // TODO
    
    public IMemberSet<GenericParameterConstraint> Constraints;
    
    public GenericParameter Clone(ParentInfo info)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return FullName;
    }
}
