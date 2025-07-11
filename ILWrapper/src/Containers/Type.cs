﻿using ILWrapper.Members;
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
    
    /// <summary>
    /// Wrapper for <see cref="TryChangeAssembly(Mono.Cecil.TypeReference,Mono.Cecil.AssemblyNameReference,Mono.Cecil.IMetadataResolver,out Mono.Cecil.TypeReference)"/>
    /// </summary>
    public bool TryChangeAssembly(AssemblyNameReference newRuntimeAssembly, IMetadataResolver resolver, out Type modifiedType)
    {
        var success = TryChangeAssembly(Base, newRuntimeAssembly, resolver, out var result);
        modifiedType = new Type(result);
        return success;
    }
    
    /// <summary>
    /// Changes the assembly of a <see cref="TypeReference"/>, ensuring the referenced type actually exists in that
    /// assembly.
    /// </summary>
    /// <param name="originalType">the type to change the assembly of</param>
    /// <param name="newAssembly">the new assembly</param>
    /// <param name="resolver">an <see cref="IMetadataResolver"/> that can resolve types in the
    /// <paramref name="newAssembly"/>. Used to resolve the modified <see cref="TypeReference"/></param>
    /// <param name="modifiedType">[out] the modified <see cref="TypeReference"/></param>
    /// <returns>whether the <paramref name="originalType"/> was moved into the <see cref="newAssembly"/></returns>
    public static bool TryChangeAssembly(TypeReference originalType, AssemblyNameReference newAssembly,
        IMetadataResolver resolver, out TypeReference modifiedType)
    {
        if (originalType is TypeSpecification specification)
        {
            var originalScope = specification.ElementType.Scope;
            specification.ElementType.Scope = newAssembly;
            TypeReference remappedRawType = resolver.Resolve(originalType);
            specification.ElementType.Scope = originalScope; // revert the scope modification
            if (remappedRawType == null)
            {
                modifiedType = originalType;
                return false;
            }
            remappedRawType.Scope = newAssembly;
            switch (originalType)
            {
                default:
                    modifiedType = remappedRawType;
                    break;
                case GenericInstanceType generic:
                {
                    modifiedType = new GenericInstanceType(remappedRawType);
                
                    foreach (var arg in generic.GenericArguments)
                    {
                        ((GenericInstanceType)modifiedType).GenericArguments.Add(arg);
                    }
                    
                    break;
                }
                case ArrayType array:
                    modifiedType = new ArrayType(remappedRawType);
                    for (var i = 0; i < array.Dimensions.Count; i++)
                    {
                        ((ArrayType)modifiedType).Dimensions[i] = array.Dimensions[i];
                    }
                    break;
                case PointerType:
                    modifiedType = new PointerType(remappedRawType);
                    break;
                case PinnedType:
                    modifiedType = new PinnedType(remappedRawType);
                    break;
                case SentinelType:
                    modifiedType = new SentinelType(remappedRawType);
                    break;
                case RequiredModifierType reqModifier:
                    modifiedType = new RequiredModifierType(reqModifier.ModifierType, remappedRawType);
                    break;
                case OptionalModifierType opModifier:
                    modifiedType = new OptionalModifierType(opModifier.ModifierType, remappedRawType);
                    break;
            }
        }
        else
        {
            var originalScope = originalType.Scope;
            originalType.Scope = newAssembly;
            modifiedType = resolver.Resolve(originalType);
            originalType.Scope = originalScope; // revert the scope modification
            if (modifiedType == null)
            {
                modifiedType = originalType;
                return false;
            }
            
            modifiedType.Scope = newAssembly;
        }
        
        return true;
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
