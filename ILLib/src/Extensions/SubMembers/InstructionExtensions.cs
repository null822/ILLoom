using ILLib.Extensions.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ILLib.Extensions.SubMembers;

public static class InstructionExtensions
{
    public static Instruction Create(OpCode opCode, object? operand = null)
    {
        var instruction = Instruction.Create(OpCodes.Nop);
        instruction.OpCode = opCode;
        instruction.Operand = operand;
        return instruction;
    }
    
    public static T GetOperand<T>(this Instruction self)
    {
        return (T)self.Operand;
    }
    
    public static Instruction Clone(this Instruction self, ParentInfo info)
    {
        if (info.Module is not ModuleDefinition module)
            throw new MissingParentInfoException(ParentInfoType.Module);
        
        var operand = self.Operand;
        
        // remap hoists
        switch (operand)
        {
            case TypeReference typeRef:
                operand = info.RemapRef(typeRef);
                break;
            case MethodReference methodRef:
            {
                var newMethodRef = info.RemapRef(methodRef);
                newMethodRef.ReturnType = info.RemapRef(newMethodRef.ReturnType);
                
                // ensure static methods are invoked statically, and vice versa
                if (newMethodRef.HasThis != methodRef.HasThis)
                    newMethodRef.HasThis = methodRef.HasThis;
                
                operand = newMethodRef;
                break;
            }
            case FieldReference fieldRef:
                var newFieldRef = info.RemapRef(fieldRef);
                newFieldRef.FieldType = info.RemapRef(newFieldRef.FieldType);
                operand = newFieldRef;
                break;
        }
        
        // remap references to the standard assembly
        if (info.RuntimeAssembly != null && module.MetadataResolver != null)
        {
            operand = RemapToNewRuntime(operand, info.RuntimeAssembly, module.MetadataResolver);
            switch (operand)
            {
                case MethodReference methodReference:
                    methodReference.ReturnType = RemapToNewRuntime(methodReference.ReturnType, info.RuntimeAssembly, module.MetadataResolver);
                    break;
                case FieldReference fieldReference:
                    fieldReference.FieldType = RemapToNewRuntime(fieldReference.FieldType, info.RuntimeAssembly, module.MetadataResolver);
                    break;
            }
        }
        
        // import references
        switch (operand)
        {
            case TypeReference typeRef:
                operand = module.ImportReference(typeRef);
                break;
            case MethodReference methodRef:
            {
                methodRef.ReturnType = module.ImportReference(methodRef.ReturnType); 
                var newMethodRef = module.ImportReference(methodRef);
            
                if (newMethodRef.HasThis != methodRef.HasThis) // ensure static methods are invoked statically, and vice versa
                    newMethodRef.HasThis = methodRef.HasThis;
                operand = newMethodRef;
                break;
            }
            case FieldReference fieldRef:
                fieldRef.FieldType = module.ImportReference(fieldRef.FieldType);
                operand = module.ImportReference(fieldRef);
                break;
        }

        var clone = Create(self.OpCode, operand);
        clone.Offset = self.Offset;
        
        return clone;
    }
    
    /// <summary>
    /// Remaps all references to the original standard library in the <paramref name="operand"/> to a new standard
    /// library. The object passed into <paramref name="operand"/> gets modified.
    /// </summary>
    /// <param name="operand">the operand to remap</param>
    /// <param name="newRuntimeAssembly">the new runtime assembly name</param>
    /// <param name="resolver">an <see cref="IMetadataResolver"/> to resolve the references</param>
    /// <typeparam name="T">the type of the operand</typeparam>
    private static T RemapToNewRuntime<T>(T operand, AssemblyNameReference newRuntimeAssembly, IMetadataResolver resolver)
    {
        if (operand is not MemberReference memberRef)
            return operand;
        
        if (operand is TypeReference typeReference)
        {
            typeReference.TryChangeAssembly(newRuntimeAssembly, resolver, out var remappedType);
            memberRef = remappedType;
        }
        else
        {
            memberRef.DeclaringType.TryChangeAssembly(newRuntimeAssembly, resolver, out var remappedType);
            if (memberRef is GenericInstanceMethod genericInstanceMethod)
            {
                genericInstanceMethod.ElementMethod.DeclaringType = remappedType;
            }
            else
            {
                memberRef.DeclaringType = remappedType;
            }
        }
        
        return (T)(object)memberRef;
    }
}