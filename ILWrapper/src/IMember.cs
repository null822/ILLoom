using ILWrapper.Members;
using Mono.Cecil;
using CustomAttribute = ILWrapper.SubMembers.CustomAttribute;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper;

public interface IMember<out TSelf, TBase> where TSelf : IMember<TSelf, TBase>
{
    internal TBase Base { get; }
    
    internal static abstract TypeConvert<TBase, TSelf> FromBase { get; }
    internal static readonly TypeConvert<TSelf, TBase> ToBase = instance => instance.Base;
    
    // public string Signature { get; }
    public string FullName { get; }

    public ParentInfo Info => new();
    
    internal static TSelf Create(TBase? @base) => @base == null ? default! : TSelf.FromBase(@base);
    
    public TSelf Clone(ParentInfo info);

}

public interface IAssemblyObject
{
    
}

public interface IMember
{
    public IMemberSet<CustomAttribute> CustomAttributes { get; }
    public MemberReference MemberBase { get; }

    public static IMember FromBaseRef(MemberReference baseReference)
    {
        return baseReference switch
        {
            TypeReference @base => new Type(@base),
            MethodReference @base => new Method(@base),
            FieldReference @base => new Field(@base),
            PropertyReference @base => new Property(@base.Resolve()),
            EventReference @base => new Event(@base.Resolve()),
            _ => throw new Exception($"{nameof(MemberReference)} of type {baseReference.GetType()} is not implemented")
        };
    }
}

public interface ISubMember;
public interface IMemberContainer;
