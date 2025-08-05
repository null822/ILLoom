using Mono.Cecil.Cil;

namespace ILLib.Extensions.SubMembers;

public static class MethodBodyExtensions
{
    public static MethodBody Clone(this MethodBody self, ParentInfo info)
    {
        MissingParentInfoException.ThrowIfMissing(info, ParentInfoType.Method);
        if (info.Method?.Resolve() is not { } method)
            throw new MissingParentInfoException(ParentInfoType.Method);
        
        var clone = new MethodBody(method)
        {
            MaxStackSize = self.MaxStackSize,
            LocalVarToken = self.LocalVarToken,
            InitLocals = self.InitLocals
        };
        info.MethodBody = clone;
        clone.Instructions.ReplaceContents(self.Instructions, info);
        clone.Variables.ReplaceContents(self.Variables, info);
        clone.ExceptionHandlers.ReplaceContents(self.ExceptionHandlers, info);
        
        return clone;
    }
    
    public static void BindReferencedInstructions(this MethodBody self)
    {
        foreach (var instruction in self.Instructions)
        {
            if (instruction.Operand is Instruction referencedInstruction)
            {
                instruction.Operand = self.Instructions[self.GetInstructionIndex(referencedInstruction.Offset)];
            }
        }
    }
    
    public static int GetInstructionIndex(this MethodBody self, int ilOffset)
    {
        var runningIlOffset = 0;
        for (var i = 0; i < self.Instructions.Count; i++)
        {
            if (runningIlOffset == ilOffset)
                return i;
            if (runningIlOffset > ilOffset)
                break;
            runningIlOffset += self.Instructions[i].GetSize();
        }
        
        throw new Exception($"Invalid IL Offset {ilOffset} for {nameof(MethodBody)} {self}");
    }
    
    public static void InsertInstructions(this MethodBody self, int index, IList<Instruction> instructions, ParentInfo? info = null,
        bool fixReferencedIlOffsets = false)
    {
        for (var i = 0; i < instructions.Count; i++)
        {
            var instruction = info == null ? instructions[i] : instructions[i].Clone((ParentInfo)info);
            self.Instructions.Insert(i + index, instruction);
        }
        
        self.AddIlOffsets(index, instructions);
        
        if (!fixReferencedIlOffsets) return;
        
        // fix referenced IL offsets in added section
        var precedingIlOffset = 0;
        for (var i = 0; i < index; i++)
        {
            precedingIlOffset += self.Instructions[i].GetSize();
        }
        for (var i = index; i < index + instructions.Count; i++)
        {
            if (self.Instructions[i].Operand is Instruction referencedInstruction)
            {
                self.Instructions[i].Operand = self.Instructions[self.GetInstructionIndex(referencedInstruction.Offset + precedingIlOffset)];
            }
        }
    }
    
    public static void InsertInstruction(this MethodBody self, int index, Instruction instruction, ParentInfo? info = null)
    {
        var clone = info == null ? instruction : instruction.Clone((ParentInfo)info);
        self.Instructions.Insert(index, clone);
        
        self.AddIlOffsets(index, [instruction]);
    }
    
    public static void RemoveInstruction(this MethodBody self, int instructionIndex)
    {
        var size = self.Instructions[instructionIndex].GetSize();
        self.Instructions.RemoveAt(instructionIndex);

        self.RemoveIlOffsets(instructionIndex, size);
    }
    
    public static void ReplaceInstruction(this MethodBody self, int index, Instruction instruction, ParentInfo? info = null)
    {
        self.RemoveInstruction(index);
        self.InsertInstruction(index, instruction, info);
    }
    
    private static void AddIlOffsets(this MethodBody self, int addedIndex, IList<Instruction> addedInstructions)
    {
        var addedIlOffset = addedInstructions.Sum(instruction => instruction.GetSize());
        var precedingIlOffset = 0;
        for (var i = 0; i < addedIndex; i++)
        {
            precedingIlOffset += self.Instructions[i].GetSize();
        }
        
        // fix IL offsets in added section
        for (var i = addedIndex; i < addedIndex + addedInstructions.Count; i++)
        {
            self.Instructions[i].Offset += precedingIlOffset;
        }
        
        // fix IL offsets after added section
        for (var i = addedIndex + addedInstructions.Count; i < self.Instructions.Count; i++)
        {
            self.Instructions[i].Offset += addedIlOffset;
        }
    }
    
    private static void RemoveIlOffsets(this MethodBody self, int afterRemovedIndex, int removedSize)
    {
        if (afterRemovedIndex > self.Instructions.Count)
            return;
        
        for (var i = afterRemovedIndex; i < self.Instructions.Count; i++)
        {
            self.Instructions[i].Offset -= removedSize;
        }
    }
    
}