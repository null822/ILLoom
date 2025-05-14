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
    
    public override string ToString()
    {
        return FullName;
    }
}
