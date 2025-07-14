using ILLoom.Transformers.TransformerTypes;
using ILLib.Extensions;
using ILLib.Extensions.SubMembers;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ILLoom.Transformers.Injectors;

public class MethodInjector(MethodDefinition injector, MethodDefinition target, IInjectLocation[] locations) : IInjector
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
                        instruction = InstructionExtensions.Create(OpCodes.Nop); // TODO: remove instruction
                    else
                        instruction = InstructionExtensions.Create(OpCodes.Br, target.Body.Instructions[injectEnd]);
                }
                
                if (instruction.OpCode == OpCodes.Call
                    && instruction.Operand is MethodReference { 
                        Name: "Return", DeclaringType.FullName: "LoomModLib.Injector" })
                {
                    instruction = InstructionExtensions.Create(OpCodes.Ret);
                }
                
                if (instruction.Operand is MemberReference memberReference)
                {
                    instruction.Operand = Program.Remap(memberReference);
                }
                
                if (instruction.Operand is TypeReference typeReference)
                {
                    instruction.Operand = target.Module.ImportReference(typeReference);
                }
                else
                {
                    if (instruction.Operand is MethodReference methodReference)
                    {
                        methodReference.ReturnType = target.Module.ImportReference(methodReference.ReturnType);
                        instruction.Operand = target.Module.ImportReference(methodReference);
                    }
                    else if (instruction.Operand is FieldReference fieldReference)
                    {
                        fieldReference.FieldType = target.Module.ImportReference(fieldReference.FieldType);
                        instruction.Operand = target.Module.ImportReference(fieldReference);
                    }
                }
                
                target.Body.ReplaceInstruction(i, instruction);
            }
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
            Code.Ldloc => instr.GetOperand<VariableReference>().Index,
            Code.Ldloc_S => instr.GetOperand<VariableReference>().Index
        };
        
        instr.OpCode = OpCodes.Ldloc;
        instr.Operand = target.Body.Variables[originalIndex + offset];
        
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
            Code.Stloc => instr.GetOperand<VariableReference>().Index,
            Code.Stloc_S => instr.GetOperand<VariableReference>().Index
        };
        
        instr.OpCode = OpCodes.Stloc;
        instr.Operand = target.Body.Variables[originalIndex + offset];
        
        return instr;
    }
    
    private Instruction OffsetLdLoca(Instruction instr, int offset)
    {
        var originalIndex = instr.GetOperand<VariableReference>().Index;
        
        instr.OpCode = OpCodes.Ldloca;
        instr.Operand = target.Body.Variables[originalIndex + offset];
        
        return instr;
    }
    
}
