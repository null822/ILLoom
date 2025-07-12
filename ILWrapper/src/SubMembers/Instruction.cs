using ILWrapper.MemberSet;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper.SubMembers;

public class Instruction : IMember<Instruction, Mono.Cecil.Cil.Instruction>, ISubMember
{
    public Mono.Cecil.Cil.Instruction Base { get; }
    static TypeConvert<Mono.Cecil.Cil.Instruction, Instruction> IMember<Instruction, Mono.Cecil.Cil.Instruction>.FromBase => instance => new Instruction(instance);
    
    public Instruction(Mono.Cecil.Cil.Instruction @base)
    {
        Base = @base;
    }

    public Instruction() : this(Mono.Cecil.Cil.Instruction.Create(OpCodes.Nop)) {}

    public Instruction(OpCode opCode, object? operand = null) : this()
    {
        OpCode = opCode;
        Operand = operand;
    }
    
    public string FullName => $"[{Offset}] {OpCode}{(Operand == null ? "" : $" {Operand}")}";
    
    public int Size => Base.GetSize();
    
    public int Offset { get => Base.Offset; set => Base.Offset = value; }
    public OpCode OpCode { get => Base.OpCode; set => Base.OpCode = value; }
    public object? Operand { get => Base.Operand; set => Base.Operand = value; }
    
    public Instruction Clone(ParentInfo info)
    {
        MissingParentInfoException.ThrowIfMissing(info, ParentInfoType.Module);
        var module = info.Module!;

        var operand = Operand;
        
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
                operand = module.Base.ImportReference(typeRef);
                break;
            case MethodReference methodRef:
            {
                methodRef.ReturnType = module.Base.ImportReference(methodRef.ReturnType); 
                var newMethodRef = module.Base.ImportReference(methodRef);
            
                if (newMethodRef.HasThis != methodRef.HasThis) // ensure static methods are invoked statically, and vice versa
                    newMethodRef.HasThis = methodRef.HasThis;
                operand = newMethodRef;
                break;
            }
            case FieldReference fieldRef:
                fieldRef.FieldType = module.Base.ImportReference(fieldRef.FieldType);
                operand = module.Base.ImportReference(fieldRef);
                break;
        }
        
        var clone = new Instruction
        {
            OpCode = OpCode,
            Operand = operand,
            Offset = Offset
        };
        
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
            Type.TryChangeAssembly(typeReference, newRuntimeAssembly, resolver, out var remappedType);
            memberRef = remappedType;
        }
        else
        {
            Type.TryChangeAssembly(memberRef.DeclaringType, newRuntimeAssembly, resolver, out var remappedType);
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
    
    public override string ToString()
    {
        return FullName;
    }
}
