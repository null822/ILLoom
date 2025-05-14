
using ILWrapper.MemberSet;
using Mono.Cecil;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper.SubMembers;

public class CustomAttributeArg : IMember<CustomAttributeArg, CustomAttributeArgument>, ISubMember
{
    public CustomAttributeArgument Base { get; }
    static TypeConvert<CustomAttributeArgument, CustomAttributeArg> IMember<CustomAttributeArg, CustomAttributeArgument>.FromBase => instance => new CustomAttributeArg(instance);
    
    public CustomAttributeArg(CustomAttributeArgument @base)
    {
        Base = @base;
    }
    
    public CustomAttributeArg(Type type, object value) : this(new CustomAttributeArgument(type.Base, value)) {}

    public string FullName => $"[({Type.FullName} _ = {Value})]";
    
    public Type Type => new(Base.Type);
    public object Value => Base.Value;

    public CustomAttributeArg Clone(ParentInfo info)
    {
        var clone = new CustomAttributeArg(info.Remap(Type), Value);
        return clone;
    }
    
    public override string ToString()
    {
        return FullName;
    }
}
