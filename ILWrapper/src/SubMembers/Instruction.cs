using ILWrapper.Containers;
using ILWrapper.Members;
using ILWrapper.MemberSet;
using Mono.Cecil;
using Mono.Cecil.Cil;

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
    public Instruction(OpCode opCode) : this(Mono.Cecil.Cil.Instruction.Create(opCode)) {}
    
    private static Mono.Cecil.Cil.Instruction[] InstrArrayToBase(Instruction[] instrArray)
    {
        var newArray = new Mono.Cecil.Cil.Instruction[instrArray.Length];
        for (var i = 0; i < instrArray.Length; i++)
        {
            newArray[i] = instrArray[i].Base;
        }
        return newArray;
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
        
        if (Operand is TypeReference typeRef)
        {
            operand = module.Base.ImportReference(info.RemapRef(typeRef));
        }
        else if (Operand is MethodReference methodRef)
        {
            var newMethodRef = module.Base.ImportReference(info.RemapRef(methodRef));
            if (newMethodRef.HasThis != methodRef.HasThis) // ensure static methods are invoked statically, and vice versa
                newMethodRef.HasThis = methodRef.HasThis;
            operand = newMethodRef;
        }
        else if (Operand is FieldReference fieldRef)
        {
            operand = module.Base.ImportReference(info.RemapRef(fieldRef));
        }

        var clone = new Instruction
        {
            OpCode = OpCode,
            Operand = operand,
            Offset = Offset
        };
        
        return clone;
    }
    
    public override string ToString()
    {
        return FullName;
    }
}
