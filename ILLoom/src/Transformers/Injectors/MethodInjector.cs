using ILLoom.Transformers.TransformerTypes;
using ILWrapper.Members;
using ILWrapper.SubMembers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Instruction = ILWrapper.SubMembers.Instruction;

namespace ILLoom.Transformers.Injectors;

public class MethodInjector(Method injector, Method target, IInjectLocation[] locations) : IInjector
{
    public string Name => $"{injector.DeclaringType?.FullName}::{injector.Name}";
    
    public InjectorApplyState Inject()
    {
        target.Body.BindReferencedInstructions();
        
        var variableOffset = target.Body.Variables.Count;
        injector.Body.Variables.CloneTo(target.Body.Variables, Program.TargetInfo);
        
        foreach (var location in locations)
        {
            var startIndex = location.ResolveInstructionIndex(target.Body);
            
            var instructions = new List<Instruction>(injector.Body.Instructions.Count - 1); // don't copy the return at the end of the method
            for (var i = 0; i < injector.Body.Instructions.Count - 1; i++)
            {
                var instruction = injector.Body.Instructions[i];
                instructions.Add(instruction);
            }
            
            target.Body.InsertInstructions(startIndex, instructions, Program.TargetInfo.With(target.Body),
                true);
            
            var injectEnd = startIndex + instructions.Count;
            for (var i = startIndex; i < startIndex + instructions.Count; i++)
            {
                var instruction = target.Body.Instructions[i];

                instruction.OpCode = instruction.OpCode.Code switch
                {
                    Code.Ldarg_0 => OpCodes.Ldarg_0,
                    Code.Ldarg_1 => OpCodes.Ldarg_0,
                    Code.Ldarg_2 => OpCodes.Ldarg_1,
                    Code.Ldarg_3 => OpCodes.Ldarg_2,

                    _ => instruction.OpCode
                };

                instruction = instruction.OpCode.Code switch
                {
                    Code.Ldloc_0 => OffsetLdLoc(instruction, variableOffset),
                    Code.Ldloc_1 => OffsetLdLoc(instruction, variableOffset),
                    Code.Ldloc_2 => OffsetLdLoc(instruction, variableOffset),
                    Code.Ldloc_3 => OffsetLdLoc(instruction, variableOffset),
                    Code.Ldloc_S => OffsetLdLoc(instruction, variableOffset),
                    Code.Ldloc => OffsetLdLoc(instruction, variableOffset),

                    Code.Stloc_0 => OffsetStLoc(instruction, variableOffset),
                    Code.Stloc_1 => OffsetStLoc(instruction, variableOffset),
                    Code.Stloc_2 => OffsetStLoc(instruction, variableOffset),
                    Code.Stloc_3 => OffsetStLoc(instruction, variableOffset),
                    Code.Stloc_S => OffsetStLoc(instruction, variableOffset),
                    Code.Stloc => OffsetStLoc(instruction, variableOffset),

                    Code.Ldloca_S => OffsetLdLoca(instruction, variableOffset),
                    Code.Ldloca => OffsetLdLoca(instruction, variableOffset),

                    _ => instruction
                };
                
                if (instruction.OpCode == OpCodes.Ret)
                {
                    if (i == injectEnd - 1)
                        instruction = new Instruction(OpCodes.Nop); // TODO: remove instruction
                    else
                        instruction = new Instruction(OpCodes.Br, target.Body.Instructions[injectEnd].Base);
                }
                
                if (instruction.OpCode == OpCodes.Call
                    && instruction.Operand is MethodReference { 
                        Name: "Return", DeclaringType.FullName: "LoomModLib.Injector" })
                {
                    instruction = new Instruction(OpCodes.Ret);
                }

                if (instruction.Operand is MemberReference m)
                {
                    instruction.Operand = Program.Remap(m);
                }
                
                target.Body.ReplaceInstruction(i, instruction);
            }
            
            // inject the instructions
            // target.Body.AddInstructions(injectorInstructions, offset, Program.TargetInfo.With(target.Body));
        }

        return InjectorApplyState.Succeeded;
    }

    private Instruction OffsetLdLoc(Instruction instr, int offset)
    {
        var originalIndex = instr.OpCode.Code switch
        {
            Code.Ldloc_0 => 0,
            Code.Ldloc_1 => 1,
            Code.Ldloc_2 => 2,
            Code.Ldloc_3 => 3,
            Code.Ldloc => new Variable((VariableDefinition)instr.Operand!).Index,
            Code.Ldloc_S => new Variable((VariableDefinition)instr.Operand!).Index
        };
        
        instr.OpCode = OpCodes.Ldloc;
        instr.Operand = target.Body.Variables[originalIndex + offset].Base;
        
        return instr;
    }
    
    private Instruction OffsetStLoc(Instruction instr, int offset)
    {
        var originalIndex = instr.OpCode.Code switch
        {
            Code.Stloc_0 => 0,
            Code.Stloc_1 => 1,
            Code.Stloc_2 => 2,
            Code.Stloc_3 => 3,
            Code.Stloc => new Variable((VariableDefinition)instr.Operand!).Index,
            Code.Stloc_S => new Variable((VariableDefinition)instr.Operand!).Index
        };
        
        instr.OpCode = OpCodes.Stloc;
        instr.Operand = target.Body.Variables[originalIndex + offset].Base;
        
        return instr;
    }
    
    private Instruction OffsetLdLoca(Instruction instr, int offset)
    {
        var originalIndex = new Variable((VariableDefinition)instr.Operand!).Index;
        
        instr.OpCode = OpCodes.Ldloca;
        instr.Operand = target.Body.Variables[originalIndex + offset].Base;
        
        return instr;
    }
    
}
