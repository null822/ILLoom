using ILWrapper.Containers;
using ILWrapper.Members;
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

    public Instruction(OpCode opCode) : this(Mono.Cecil.Cil.Instruction.Create(opCode)) {}
    public Instruction(OpCode opCode, CallSite value) : this(Mono.Cecil.Cil.Instruction.Create(opCode, value)) {}
    public Instruction(OpCode opCode, string value) : this(Mono.Cecil.Cil.Instruction.Create(opCode, value)) {}
    public Instruction(OpCode opCode, sbyte value) : this(Mono.Cecil.Cil.Instruction.Create(opCode, value)) {}
    public Instruction(OpCode opCode, byte value) : this(Mono.Cecil.Cil.Instruction.Create(opCode, value)) {}
    public Instruction(OpCode opCode, int value) : this(Mono.Cecil.Cil.Instruction.Create(opCode, value)) {}
    public Instruction(OpCode opCode, long value) : this(Mono.Cecil.Cil.Instruction.Create(opCode, value)) {}
    public Instruction(OpCode opCode, float value) : this(Mono.Cecil.Cil.Instruction.Create(opCode, value)) {}
    public Instruction(OpCode opCode, double value) : this(Mono.Cecil.Cil.Instruction.Create(opCode, value)) {}
    public Instruction(OpCode opCode, Instruction target) : this(Mono.Cecil.Cil.Instruction.Create(opCode, target.Base)) {}
    public Instruction(OpCode opCode, Instruction[] targets) : this(Mono.Cecil.Cil.Instruction.Create(opCode, InstrArrayToBase(targets))) {}
    public Instruction(OpCode opCode, Method method) : this(Mono.Cecil.Cil.Instruction.Create(opCode, method.Base)) {}
    public Instruction(OpCode opCode, Field field) : this(Mono.Cecil.Cil.Instruction.Create(opCode, field.Base)) {}
    public Instruction(OpCode opCode, Variable variable) : this(Mono.Cecil.Cil.Instruction.Create(opCode, variable.Base)) {}
    public Instruction(OpCode opCode, Parameter value) : this(Mono.Cecil.Cil.Instruction.Create(opCode, value.Base)) {}
    
    internal Instruction(OpCode opCode, Mono.Cecil.Cil.Instruction target) : this(Mono.Cecil.Cil.Instruction.Create(opCode, target)) {}
    internal Instruction(OpCode opCode, Mono.Cecil.Cil.Instruction[] targets) : this(Mono.Cecil.Cil.Instruction.Create(opCode, targets)) {}
    internal Instruction(OpCode opCode, VariableDefinition variable) : this(Mono.Cecil.Cil.Instruction.Create(opCode, variable)) {}
    internal Instruction(OpCode opCode, ParameterDefinition parameter) : this(Mono.Cecil.Cil.Instruction.Create(opCode, parameter)) {}
    
    internal Instruction(OpCode opCode, TypeReference type, Module module) : this(Mono.Cecil.Cil.Instruction.Create(opCode, module.Base.ImportReference(type))) {}
    internal Instruction(OpCode opCode, MethodReference method, Module module) : this(Mono.Cecil.Cil.Instruction.Create(opCode, module.Base.ImportReference(method))) {}
    internal Instruction(OpCode opCode, FieldReference field, Module module) : this(Mono.Cecil.Cil.Instruction.Create(opCode, module.Base.ImportReference(field))) {}
    
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
        
        var clone = Operand switch
        {
            string v => new Instruction(OpCode, v),
            sbyte v => new Instruction(OpCode, v),
            byte v => new Instruction(OpCode, v),
            int v => new Instruction(OpCode, v),
            long v => new Instruction(OpCode, v),
            float v => new Instruction(OpCode, v),
            double v => new Instruction(OpCode, v),
            
            Mono.Cecil.Cil.Instruction v => new Instruction(OpCode, v),
            Mono.Cecil.Cil.Instruction[] v => new Instruction(OpCode, v),
            VariableDefinition v => new Instruction(OpCode, v),
            ParameterDefinition v => new Instruction(OpCode, v),
            
            TypeReference v => new Instruction(OpCode, info.RemapRef(v), module),
            MethodReference v => new Instruction(OpCode, info.RemapRef(v), module),
            FieldReference v => new Instruction(OpCode, info.RemapRef(v), module),
            
            _ => new Instruction(OpCode)
        };
        // Console.WriteLine($"{Operand?.GetType()} {Operand} => {clone.Operand} in {info.Module}");
        clone.Offset = Offset;
        
        return clone;
    }
    
    public override string ToString()
    {
        return FullName;
    }
}
