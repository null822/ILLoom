
using Mono.Cecil;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper.SubMembers;

public class CustomAttributeNamedArg : IMember<CustomAttributeNamedArg, CustomAttributeNamedArgument>, ISubMember
{
    public CustomAttributeNamedArgument Base { get; }
    static TypeConvert<CustomAttributeNamedArgument, CustomAttributeNamedArg> IMember<CustomAttributeNamedArg, CustomAttributeNamedArgument>.FromBase => instance => new CustomAttributeNamedArg(instance);
    
    public CustomAttributeNamedArg(CustomAttributeNamedArgument @base)
    {
        Base = @base;
    }

    public CustomAttributeNamedArg(string name, CustomAttributeArg arg) : this(new CustomAttributeNamedArgument(name, arg.Base)) {}
    public CustomAttributeNamedArg(string name, Type type, object value) : this(new CustomAttributeNamedArgument(name, new CustomAttributeArgument(type.Base, value))) {}

    public string Signature => $"CuANArg : {nameof(CustomAttributeNamedArg)}: {Name}";
    public string FullName => $"[({Type.FullName} {Name} = {Value})]";
    public string Name => Base.Name;
    
    public CustomAttributeArg Arg => IMember<CustomAttributeArg, CustomAttributeArgument>.Create(Base.Argument);
    
    public Type Type => Arg.Type;
    public object Value => Arg.Value;
    
    public CustomAttributeNamedArg Clone(ParentInfo info)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return FullName;
    }
}
