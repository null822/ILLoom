using ILWrapper.Containers;
using ILWrapper.SubMembers;
using Mono.Cecil;
using CustomAttribute = ILWrapper.SubMembers.CustomAttribute;
using GenericParameter = ILWrapper.SubMembers.GenericParameter;
using MethodBody = ILWrapper.SubMembers.MethodBody;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper.Members;

public class Method : IMember<Method, MethodReference>, IMember
{
    public MethodReference Base { get; }
    public MemberReference MemberBase => Base;
    static TypeConvert<MethodReference, Method> IMember<Method, MethodReference>.FromBase => instance => new Method(instance);
    
    public ParentInfo Info { get; }
    
    public Method(MethodReference @base)
    {
        Base = @base;
        
        Parameters = new MemberSet<Parameter, ParameterDefinition>(Base.Parameters);
        GenericParameters = new MemberSet<GenericParameter, Mono.Cecil.GenericParameter>(Base.GenericParameters);
        CustomAttributes = new MemberSet<CustomAttribute, Mono.Cecil.CustomAttribute>(Base.Resolve().CustomAttributes);
        Overrides = new MemberSet<Method, MethodReference>(Base.Resolve().Overrides);

        Info = new ParentInfo(this);
    }
    
    public Method(string name, MethodAttributes attributes, Type returnType) : this(new MethodDefinition(name, attributes, returnType.Base)) {}
    
    public string Name { get => Base.Name; set => Base.Name = value; }
    public string FullName => Base.FullName;
    
    public Module Module => IMember<Module, ModuleDefinition>.Create(Base.Module);
    
    public Type ReturnType { get => new(Base.ReturnType); set => Base.ReturnType = value.Base; }
    public Type DeclaringType { get => IMember<Type, TypeReference>.Create(Base.DeclaringType); set => Base.DeclaringType = value.Base; }
    public MethodBody Body { get => IMember<MethodBody, Mono.Cecil.Cil.MethodBody>.Create(Base.Resolve().Body); set => Base.Resolve().Body = value?.Base; }
    public MethodAttributes Attributes { get => Base.Resolve().Attributes; set => Base.Resolve().Attributes = value; }
    public MetadataToken MetadataToken { get => Base.MetadataToken; set => Base.MetadataToken = value; }
    
    public IMemberSet<CustomAttribute> CustomAttributes { get; }
    public IMemberSet<Parameter> Parameters { get; }
    public IMemberSet<GenericParameter> GenericParameters { get; }
    public IMemberSet<Method> Overrides { get; }
    // CustomDebugInformation
    
    public Method Clone(ParentInfo info)
    {
        info.RequireTypes(ParentInfoType.Type);
        var clone = new Method(Name, Attributes, info.Remap(ReturnType))
        {
            DeclaringType = info.Type!,
            Attributes = Attributes,
            MetadataToken = MetadataToken
        };
        info.Method = clone;
        clone.Body = Body.Clone(info);
        clone.Parameters.ReplaceContents(Parameters, info);
        clone.GenericParameters.ReplaceContents(GenericParameters, info);
        clone.CustomAttributes.ReplaceContents(CustomAttributes, info);
        clone.Overrides.ReplaceContents(Overrides, info);
        
        return clone;
    }

    public override string ToString()
    {
        return FullName;
    }
}
