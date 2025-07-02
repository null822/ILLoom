using ILWrapper.Members;
using ILWrapper.MemberSet;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ILWrapper.SubMembers;

public class MethodBody : IMember<MethodBody, Mono.Cecil.Cil.MethodBody>, ISubMember
{
    public Mono.Cecil.Cil.MethodBody Base { get; }
    static TypeConvert<Mono.Cecil.Cil.MethodBody, MethodBody> IMember<MethodBody, Mono.Cecil.Cil.MethodBody>.FromBase => instance => new MethodBody(instance);
    
    public MethodBody(Mono.Cecil.Cil.MethodBody @base)
    {
        Base = @base;
        
        Instructions = new MemberSet<Instruction, Mono.Cecil.Cil.Instruction>(Base.Instructions);
        Variables = new MemberSet<Variable, VariableDefinition>(Base.Variables);
        ExceptionHandlers = new MemberSet<ExceptionHandler, Mono.Cecil.Cil.ExceptionHandler>(Base.ExceptionHandlers);
    }
    
    public MethodBody(Method method) : this(new Mono.Cecil.Cil.MethodBody(method.Base.Resolve())) {}
    
    public string FullName => $"{Method.FullName}{{_}}";
    
    public Method Method => new(Base.Method);
    public int CodeSize => Base.CodeSize;
    public ILProcessor Processor => Base.GetILProcessor();
    
    public int MaxStackSize { get => Base.MaxStackSize; set => Base.MaxStackSize = value; }
    public MetadataToken LocalVarToken { get => Base.LocalVarToken; set => Base.LocalVarToken = value; }
    public bool InitLocals { get => Base.InitLocals; set => Base.InitLocals = value; }
    
    public readonly IMemberSet<Instruction> Instructions;
    public readonly IMemberSet<Variable> Variables;
    public readonly IMemberSet<ExceptionHandler> ExceptionHandlers;
    
    public MethodBody Clone(ParentInfo info)
    {
        MissingParentInfoException.ThrowIfMissing(info, ParentInfoType.Method);
        
        var clone = new MethodBody(info.Method!)
        {
            MaxStackSize = MaxStackSize,
            LocalVarToken = LocalVarToken,
            InitLocals = InitLocals
        };
        info.MethodBody = clone;
        clone.Instructions.ReplaceContents(Instructions, info);
        clone.Variables.ReplaceContents(Variables, info);
        clone.ExceptionHandlers.ReplaceContents(ExceptionHandlers, info);
        
        return clone;
    }
    
    public void BindReferencedInstructions()
    {
        for (var i = 0; i < Instructions.Count; i++)
        {
            if (Instructions[i].Operand is Mono.Cecil.Cil.Instruction referencedInstruction)
            {
                Instructions[i].Operand = Instructions[GetInstructionIndex(referencedInstruction.Offset)].Base;
            }
        }
    }
    
    public int GetInstructionIndex(int ilOffset)
    {
        var runningIlOffset = 0;
        for (var i = 0; i < Instructions.Count; i++)
        {
            var instruction = Instructions[i];
            if (runningIlOffset == ilOffset)
                return i;
            runningIlOffset += instruction.Size;
        }
        
        throw new Exception($"Invalid IL Offset {ilOffset} for {nameof(MethodBody)} {this}");
    }
    
    public void InsertInstructions(int index, IList<Instruction> instructions, ParentInfo? info = null,
        bool fixReferencedIlOffsets = false)
    {
        for (var i = 0; i < instructions.Count; i++)
        {
            var instruction = info == null ? instructions[i] : instructions[i].Clone((ParentInfo)info);
            Instructions.Insert(i + index, instruction);
        }
        
        AddIlOffsets(index, instructions);
        
        if (!fixReferencedIlOffsets) return;
        
        // fix referenced IL offsets in added section
        var precedingIlOffset = 0;
        for (var i = 0; i < index; i++)
        {
            precedingIlOffset += Instructions[i].Size;
        }
        for (var i = index; i < index + instructions.Count; i++)
        {
            if (Instructions[i].Operand is Mono.Cecil.Cil.Instruction referencedInstruction)
            {
                Instructions[i].Operand = Instructions[GetInstructionIndex(referencedInstruction.Offset + precedingIlOffset)].Base;
            }
        }
    }
    
    public void InsertInstruction(int index, Instruction instruction, ParentInfo? info = null)
    {
        var clone = info == null ? instruction : instruction.Clone((ParentInfo)info);
        Instructions.Insert(index, clone);
        
        AddIlOffsets(index, [instruction]);
    }
    
    public void RemoveInstruction(int instructionIndex)
    {
        var size = Instructions[instructionIndex].Size;
        Instructions.RemoveAt(instructionIndex);

        RemoveIlOffsets(instructionIndex, size);
    }
    
    public void ReplaceInstruction(int index, Instruction instruction, ParentInfo? info = null)
    {
        RemoveInstruction(index);
        InsertInstruction(index, instruction, info);
    }
    
    private void AddIlOffsets(int addedIndex, IList<Instruction> addedInstructions)
    {
        var addedIlOffset = addedInstructions.Sum(instruction => instruction.Size);
        var precedingIlOffset = 0;
        for (var i = 0; i < addedIndex; i++)
        {
            precedingIlOffset += Instructions[i].Size;
        }
        
        // fix IL offsets in added section
        for (var i = addedIndex; i < addedIndex + addedInstructions.Count; i++)
        {
            Instructions[i].Offset += precedingIlOffset;
        }
        
        // fix IL offsets after added section
        for (var i = addedIndex + addedInstructions.Count; i < Instructions.Count; i++)
        {
            Instructions[i].Offset += addedIlOffset;
        }
    }

    private void RemoveIlOffsets(int afterRemovedIndex, int removedSize)
    {
        if (afterRemovedIndex > Instructions.Count)
            return;
        
        for (var i = afterRemovedIndex; i < Instructions.Count; i++)
        {
            Instructions[i].Offset -= removedSize;
        }
    }
    
    public override string ToString()
    {
        return FullName;
    }
}
