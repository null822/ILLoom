using ILWrapper.Members;
using ILWrapper.MemberSet;
using Mono.Cecil;
using InterfaceImplementation = ILWrapper.SubMembers.InterfaceImplementation;

// this could also be in ILWrapper.Members due to nested types
namespace ILWrapper.Containers;

public class Type : IMember<Type, TypeReference>, IMember, IMemberContainer
{
    public TypeReference Base { get; }
    public MemberReference MemberBase => Base;
    static TypeConvert<TypeReference, Type> IMember<Type, TypeReference>.FromBase => instance => new Type(instance);
    
    public ParentInfo Info { get; }
    
    public Type(TypeReference @base)
    {
        Base = @base;

        var baseDef = Base.Resolve();
        
        Methods = MemberSet<Method, MethodReference>.From(baseDef.Methods);
        Fields = MemberSet<Field, FieldReference>.From(baseDef.Fields);
        Properties = new MemberSet<Property, PropertyDefinition>(baseDef.Properties);
        Events = new MemberSet<Event, EventDefinition>(baseDef.Events);
        Interfaces = new MemberSet<InterfaceImplementation, Mono.Cecil.InterfaceImplementation>(baseDef.Interfaces);
        NestedTypes = MemberSet<Type, TypeReference>.From(baseDef.NestedTypes);
        GenericParameters = new MemberSet<SubMembers.GenericParameter, GenericParameter>(Base.GenericParameters);
        CustomAttributes = new MemberSet<SubMembers.CustomAttribute, CustomAttribute>(baseDef.CustomAttributes);
        
        Info = new ParentInfo().With(this);
    }
    
    public Type(string? @namespace, string name, TypeAttributes attributes) :
        this(new TypeDefinition(@namespace, name, attributes)) { }
    public Type(string? @namespace, string name, TypeAttributes attributes, Type baseType) :
        this(new TypeDefinition(@namespace, name, attributes, baseType.Base)) { }
    public Type(string? @namespace, string name, Module module, IMetadataScope scope) :
        this(new TypeReference(@namespace, name, module.Base, scope)) { }

    public string Name { get => Base.Name; set => Base.Name = value; }
    public string Namespace { get => Base.Namespace; set => Base.Namespace = value; }
    public string FullName => Base.FullName;
    
    public Module Module => IMember<Module, ModuleDefinition>.Create(Base.Module);
    
    public TypeAttributes Attributes { get => Base.Resolve().Attributes; set => Base.Resolve().Attributes = value; }
    public Type? BaseType { get => IMember<Type, TypeReference>.Create(Base.Resolve().BaseType); set => Base.Resolve().BaseType = value?.Base; }
    public Type? DeclaringType { get => IMember<Type, TypeReference>.Create(Base.DeclaringType); set => Base.DeclaringType = value?.Base; }
    
    public IMemberSet<Method> Methods { get; }
    public IMemberSet<Field> Fields { get; }
    public IMemberSet<Property> Properties { get; }
    public IMemberSet<Event> Events { get; }
    public IMemberSet<InterfaceImplementation> Interfaces { get; }
    public IMemberSet<Type> NestedTypes { get; }
    public IMemberSet<SubMembers.GenericParameter> GenericParameters { get; }
    public IMemberSet<SubMembers.CustomAttribute> CustomAttributes { get; }
    
    
    public Type Clone(ParentInfo info)
    {
        info.RequireTypes(ParentInfoType.Module);
        var clone = new Type(Namespace, Name, Attributes)
        {
            BaseType = BaseType,
            DeclaringType = info.Type ?? DeclaringType
        };
        info.Type = clone;
        clone.Methods.ReplaceContents(Methods, info);
        clone.Fields.ReplaceContents(Fields, info);
        clone.Properties.ReplaceContents(Properties, info);
        clone.Events.ReplaceContents(Events, info);
        clone.Interfaces.ReplaceContents(Interfaces, info);
        clone.NestedTypes.ReplaceContents(NestedTypes, info);
        clone.GenericParameters.ReplaceContents(GenericParameters, info);
        clone.CustomAttributes.ReplaceContents(CustomAttributes, info);
        
        return clone;
    }
    
    public bool Is<T>()
    {
        return typeof(T).FullName == FullName;
    }

    public bool Implements<T>()
    {
        return Interfaces.Any(interf => interf.Type.Is<T>());
    }
    
    public bool Extends<T>()
    {
        return BaseType?.Is<T>() ?? false;
    }
    
    public bool IsEmpty(bool ignoreConstructors = false)
    {
        var methodCount = ignoreConstructors ? Methods.Count(m => !m.Base.Resolve().IsConstructor) : Methods.Count;
        
        return methodCount == 0 &&
               Fields.Count == 0 &&
               Properties.Count == 0 &&
               Events.Count == 0 &&
               NestedTypes.Count == 0;
    }

    public Type GetRootType()
    {
        var t = this;
        while (t.DeclaringType != null)
            t = t.DeclaringType;

        return t;
    }
    
    public override string ToString()
    {
        return FullName;
    }
}
