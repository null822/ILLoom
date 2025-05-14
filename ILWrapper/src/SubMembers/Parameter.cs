
using ILWrapper.Members;
using ILWrapper.MemberSet;
using Mono.Cecil;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper.SubMembers;

public class Parameter : IMember<Parameter, ParameterDefinition>, ISubMember
{
    public ParameterDefinition Base { get; }
    static TypeConvert<ParameterDefinition, Parameter> IMember<Parameter, ParameterDefinition>.FromBase => instance => new Parameter(instance);
    
    public Parameter(ParameterDefinition @base)
    {
        Base = @base;
        
        CustomAttributes = new MemberSet<CustomAttribute, Mono.Cecil.CustomAttribute>(Base.CustomAttributes);
    }
    
    public Parameter(string name, ParameterAttributes attributes, Type parameterType) : this(new ParameterDefinition(name, attributes, parameterType.Base)) {}
    
    public string Name { get => Base.Name; set => Base.Name = value; }
    public string FullName => $"{Type?.FullName},{Name}";
    
    public Type? Type { get => IMember<Type, TypeReference>.Create(Base.ParameterType); set => Base.ParameterType = value?.Base; }
    public ParameterAttributes Attributes {get => Base.Attributes; set => Base.Attributes = value; }
    public MarshalInfo MarshalInfo {get => Base.MarshalInfo; set => Base.MarshalInfo = value; }
    public object Constant { get => Base.Constant; set => Base.Constant = value; }
    public bool HasDefault { get => Base.HasDefault; set => Base.HasDefault = value; }
    public bool HasConstant { get => Base.HasConstant; set => Base.HasConstant = value; }
    public bool HasFieldMarshal { get => Base.HasFieldMarshal; set => Base.HasFieldMarshal = value; }
    public bool IsIn { get => Base.IsIn; set => Base.IsIn = value; }
    public bool IsOut { get => Base.IsOut; set => Base.IsOut = value; }
    public bool IsLcid { get => Base.IsLcid; set => Base.IsLcid = value; }
    public bool IsOptional { get => Base.IsOptional; set => Base.IsOptional = value; }
    public bool IsReturnValue { get => Base.IsReturnValue; set => Base.IsReturnValue = value; }
    
    public Method Method => IMember<Method, MethodReference>.Create((MethodReference)Base.Method)!;
    public int Index => Base.Index;
    public int Sequence => Base.Sequence;
    public bool HasCustomAttributes => Base.HasCustomAttributes;
    public bool HasMarshalInfo => Base.HasMarshalInfo;
    
    public readonly IMemberSet<CustomAttribute> CustomAttributes;
    
    public Parameter Clone(ParentInfo info)
    {
        var paremeter = new Parameter(Name, Attributes, info.Remap(Type!))
        {
            Attributes = Attributes,
            MarshalInfo = MarshalInfo,
            Constant = Constant,
            HasDefault = HasDefault,
            HasConstant = HasConstant,
            HasFieldMarshal = HasFieldMarshal,
            IsIn = IsIn,
            IsOut = IsOut,
            IsLcid = IsLcid,
            IsOptional = IsOptional,
            IsReturnValue = IsReturnValue
        };
        paremeter.CustomAttributes.ReplaceContents(CustomAttributes, info);
        // TODO: missing index
        return paremeter;
    }
    
    public override string ToString()
    {
        return FullName;
    }
}
