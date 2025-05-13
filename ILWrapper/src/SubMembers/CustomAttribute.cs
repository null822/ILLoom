
using ILWrapper.Containers;
using ILWrapper.Members;
using Mono.Cecil;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper.SubMembers;

public class CustomAttribute : IMember<CustomAttribute, Mono.Cecil.CustomAttribute>, ISubMember
{
    public Mono.Cecil.CustomAttribute Base { get; }
    static TypeConvert<Mono.Cecil.CustomAttribute, CustomAttribute> IMember<CustomAttribute, Mono.Cecil.CustomAttribute>.FromBase => instance => new CustomAttribute(instance);
    
    public CustomAttribute(Mono.Cecil.CustomAttribute @base)
    {
        Base = @base;
        
        Properties = new MemberSet<CustomAttributeNamedArg, CustomAttributeNamedArgument>(Base.Properties);
        Fields = new MemberSet<CustomAttributeNamedArg, CustomAttributeNamedArgument>(Base.Fields);
        ConstructorArguments = new MemberSet<CustomAttributeArg, CustomAttributeArgument>(Base.ConstructorArguments);
    }

    public CustomAttribute(Method constructor, byte[]? blob = null) : this(blob == null ?
        new Mono.Cecil.CustomAttribute(constructor.Base) :
        new Mono.Cecil.CustomAttribute(constructor.Base, blob)) {}
    public CustomAttribute(Method constructor, Module module, byte[]? blob = null) : this(blob == null ?
        new Mono.Cecil.CustomAttribute(module.Base.ImportReference(constructor.Base)) :
        new Mono.Cecil.CustomAttribute(module.Base.ImportReference(constructor.Base), blob)) {}

    public string FullName => $"[{Type.FullName}({ConstructorArguments.ToString(p => p.FullName)})]";
    
    public Type Type => IMember<Type, TypeReference>.Create(Base.AttributeType);
    
    public Method Constructor { get => new(Base.Constructor); set => Base.Constructor = value.Base; }
    public byte[] Blob => Base.GetBlob();
    
    public readonly IMemberSet<CustomAttributeNamedArg> Properties;
    public readonly IMemberSet<CustomAttributeNamedArg> Fields;
    public readonly IMemberSet<CustomAttributeArg> ConstructorArguments;

    public object this[int i] => ConstructorArguments[i].Value;
    
    public CustomAttribute Clone(ParentInfo info)
    {
        MissingParentInfoException.ThrowIfMissing(info, ParentInfoType.Module);
        
        var clone = new CustomAttribute(Constructor, info.Module!, Blob);
        clone.Properties.ReplaceContents(Properties, info);
        clone.Fields.ReplaceContents(Fields, info);
        clone.ConstructorArguments.ReplaceContents(ConstructorArguments, info);
        
        return clone;
    }
    
    public override string ToString()
    {
        return FullName;
    }
}
