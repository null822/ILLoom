using ILWrapper.Containers;
using ILWrapper.MemberSet;
using ILWrapper.SubMembers;
using Mono.Cecil;
using CustomAttribute = Mono.Cecil.CustomAttribute;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper.Members;

public class Property : IMember<Property, PropertyDefinition>, IMember
{
    public PropertyDefinition Base { get; }
    public MemberReference MemberBase => Base;
    static TypeConvert<PropertyDefinition, Property> IMember<Property, PropertyDefinition>.FromBase => instance => new Property(instance);
    
    public ParentInfo Info { get; }
    
    public Property(PropertyDefinition @base)
    {
        Base = @base;
        Parameters = new MemberSet<Parameter, ParameterDefinition>(Base.Resolve().Parameters);
        OtherMethods = MemberSet<Method, MethodReference>.From(Base.Resolve().OtherMethods);
        CustomAttributes = new MemberSet<SubMembers.CustomAttribute, CustomAttribute>(Base.Resolve().CustomAttributes);
        
        Info = new ParentInfo();
    }
    
    public Property(string name, PropertyAttributes attributes, Type type) : this(new PropertyDefinition(name, attributes, type.Base)) {}
    
    public string Name { get => Base.Name; set => Base.Name = value; }
    public string FullName => Base.FullName;
    
    public Module Module => IMember<Module, ModuleDefinition>.Create(Base.Module);
    
    public Type PropertyType { get => new(Base.PropertyType); set => Base.PropertyType = value?.Base; }
    public object? Constant { get => Base.Resolve().Constant; set => Base.Resolve().Constant = value; }
    public Method? Setter { get => IMember<Method, MethodReference>.Create(Base.Resolve().SetMethod); set => Base.Resolve().SetMethod = value?.Base.Resolve(); }
    public Method? Getter { get => IMember<Method, MethodReference>.Create(Base.Resolve().GetMethod); set => Base.Resolve().GetMethod = value?.Base.Resolve(); }
    public Type? DeclaringType { get => IMember<Type, TypeReference>.Create(Base.DeclaringType); set => Base.DeclaringType = value?.Base.Resolve(); }
    public PropertyAttributes Attributes { get => Base.Resolve().Attributes; set => Base.Resolve().Attributes = value; }
    public MetadataToken MetadataToken { get => Base.MetadataToken; set => Base.MetadataToken = value; }
    
    public IMemberSet<Parameter> Parameters { get; }
    public IMemberSet<Method> OtherMethods { get; }
    public IMemberSet<SubMembers.CustomAttribute> CustomAttributes { get; }

    
    public Property Clone(ParentInfo info)
    {
        info.RequireTypes(ParentInfoType.Type);
        var clone = new Property(Name, Attributes, info.Remap(PropertyType))
        {
            Constant = Constant,
            Setter = Setter?.Clone(info),
            Getter = Getter?.Clone(info),
            DeclaringType = info.Type!,
            MetadataToken = MetadataToken,
        };
        info.Property = clone;
        clone.Parameters.ReplaceContents(Parameters, info);
        clone.OtherMethods.ReplaceContents(OtherMethods, info);
        clone.CustomAttributes.ReplaceContents(CustomAttributes, info);
        
        return clone;
    }
    
    public override string ToString()
    {
        return FullName;
    }
}
