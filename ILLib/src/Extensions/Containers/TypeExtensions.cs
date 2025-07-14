using Mono.Cecil;

namespace ILLib.Extensions.Containers;

public static class TypeExtensions
{
    public static TypeDefinition Clone(this TypeDefinition self, ParentInfo info)
    {
        info.RequireTypes(ParentInfoType.Module);
        var clone = new TypeDefinition(self.Namespace, self.Name, self.Attributes)
        {
            BaseType = self.BaseType,
            DeclaringType = info.Type?.Resolve() ?? self.DeclaringType
        };
        info.Type = clone;
        clone.CustomAttributes.ReplaceContents(self.CustomAttributes, info);
        clone.Methods.ReplaceContents(self.Methods, info);
        clone.Fields.ReplaceContents(self.Fields, info);
        clone.Properties.ReplaceContents(self.Properties, info);
        clone.Events.ReplaceContents(self.Events, info);
        clone.Interfaces.ReplaceContents(self.Interfaces, info);
        clone.NestedTypes.ReplaceContents(self.NestedTypes, info);
        clone.GenericParameters.ReplaceContents(self.GenericParameters, info);
        
        return clone;
    }
    
    /// <summary>
    /// Remaps this type using the <paramref name="info"/>'s <see cref="ParentInfo.Remapper"/>,
    /// then remaps runtime assembly references to the runtime assembly specified in the <paramref name="info"/>'s
    /// <see cref="ParentInfo.RuntimeAssembly"/>,
    /// then imports the resulting type into the <paramref name="info"/>'s <see cref="ParentInfo.Module"/>.
    /// </summary>
    /// <param name="self">the type to remap and import</param>
    /// <param name="info">the <see cref="ParentInfo"/> containing the remap and import targets</param>
    /// <param name="missingRuntimeType">true when the <see cref="TypeReference"/> belongs to the runtime library, but was
    /// not found in the new one</param>
    /// <returns>the remapped runtime library, or the original if nothing was changed</returns>
    public static TypeReference RemapAndImport(this TypeReference self, ParentInfo info, out bool missingRuntimeType)
    {
        missingRuntimeType = false;
        var type = self;
        
        if (info.Module is not ModuleDefinition moduleDefinition) return self;
        
        type = info.Remap(type);
        if (info is { RuntimeAssembly: not null } && moduleDefinition.MetadataResolver != null)
        {
            var asmChanged = type.TryChangeAssembly(info.RuntimeAssembly, moduleDefinition.MetadataResolver, out type);
                
            var asmName = self.Module.Assembly.Name;
            if (!asmChanged && asmName.Name.StartsWith("System") && asmName.Version != info.RuntimeAssembly.Version)
            {
                Util.Warn($"Type [{self.FullName}] was not found in new Assembly [{info.RuntimeAssembly.Name} v{info.RuntimeAssembly.Version}]");
                missingRuntimeType = true;
            }
        }

        return moduleDefinition.TryImportReference(type);

    }
    
    /// <summary>
    /// Overload of <see cref="RemapAndImport(Mono.Cecil.TypeReference,ParentInfo,out bool)"/> without the out
    /// parameter
    /// </summary>
    public static TypeReference RemapAndImport(this TypeReference self, ParentInfo info)
    {
        return RemapAndImport(self, info, out _);
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
    public static bool TryChangeAssembly(this TypeReference originalType, AssemblyNameReference newAssembly,
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
    
    
    public static bool Is<T>(this TypeReference self)
    {
        return typeof(T).FullName == self.FullName;
    }

    public static bool Implements<T>(this TypeDefinition self)
    {
        return self.Interfaces.Any(interf => interf.InterfaceType.Is<T>());
    }
    
    public static bool Extends<T>(this TypeDefinition self)
    {
        return self.BaseType?.Is<T>() ?? false;
    }
    
    public static bool IsEmpty(this TypeDefinition self, bool ignoreConstructors = false)
    {
        var methodCount = ignoreConstructors ? self.Methods.Count(m => !m.IsConstructor) : self.Methods.Count;
        
        return methodCount == 0 &&
               self.Fields.Count == 0 &&
               self.Properties.Count == 0 &&
               self.Events.Count == 0 &&
               self.NestedTypes.Count == 0;
    }
    
    public static TypeReference GetRootType(this TypeReference self)
    {
        var t = self;
        while (t.DeclaringType != null)
            t = t.DeclaringType;

        return t;
    }
}