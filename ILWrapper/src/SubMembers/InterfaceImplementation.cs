
using Type = ILWrapper.Containers.Type;

namespace ILWrapper.SubMembers;

public class InterfaceImplementation : IMember<InterfaceImplementation, Mono.Cecil.InterfaceImplementation>, ISubMember
{
    public Mono.Cecil.InterfaceImplementation Base { get; }
    static TypeConvert<Mono.Cecil.InterfaceImplementation, InterfaceImplementation> IMember<InterfaceImplementation, Mono.Cecil.InterfaceImplementation>.FromBase => instance => new InterfaceImplementation(instance);
    
    public InterfaceImplementation(Mono.Cecil.InterfaceImplementation @base)
    {
        Base = @base;
        
        CustomAttributes = new MemberSet<CustomAttribute, Mono.Cecil.CustomAttribute>(Base.CustomAttributes);
    }
    
    public InterfaceImplementation(Type type) : this(new Mono.Cecil.InterfaceImplementation(type.Base)) {}

    public string Signature => $"IImpl   : {Type}";
    public string FullName => $": {Type}";

    public Type Type { get => new(Base.InterfaceType); set => Base.InterfaceType = value.Base; }
    
    public readonly IMemberSet<CustomAttribute> CustomAttributes;
    
    public InterfaceImplementation Clone(ParentInfo info)
    {
        MissingParentInfoException.ThrowIfMissing(info, ParentInfoType.Type);
        
        var interfaceImplementation = new InterfaceImplementation(info.Remap(Type));
        interfaceImplementation.CustomAttributes.ReplaceContents(CustomAttributes, info);
        
        return interfaceImplementation;
    }
    
    public override string ToString()
    {
        return FullName;
    }
}
