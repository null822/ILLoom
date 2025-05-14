using ILWrapper.MemberSet;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper.SubMembers;

public class ExceptionHandler : IMember<ExceptionHandler, Mono.Cecil.Cil.ExceptionHandler>, ISubMember
{
    public Mono.Cecil.Cil.ExceptionHandler Base { get; }
    static TypeConvert<Mono.Cecil.Cil.ExceptionHandler, ExceptionHandler> IMember<ExceptionHandler, Mono.Cecil.Cil.ExceptionHandler>.FromBase => instance => new ExceptionHandler(instance);
    
    public ExceptionHandler(Mono.Cecil.Cil.ExceptionHandler @base)
    {
        Base = @base;
    }

    public ExceptionHandler(ExceptionHandlerType handlerType) : this(new Mono.Cecil.Cil.ExceptionHandler(handlerType)) {}
    
    public string FullName => $"catch ({CatchType.FullName})";
    
    public Instruction FilterStart { get => new (Base.FilterStart); set => Base.FilterStart = value.Base; }
    public Instruction TryEnd { get => new (Base.TryEnd); set => Base.TryEnd = value.Base; }
    public Instruction TryStart { get => new (Base.TryStart); set => Base.TryStart = value.Base; }
    public Instruction HandlerEnd { get => new (Base.HandlerEnd); set => Base.HandlerEnd = value.Base; }
    public Instruction HandlerStart { get => new (Base.HandlerStart); set => Base.HandlerStart = value.Base; }
    public Type CatchType { get => IMember<Type, TypeReference>.Create(Base.CatchType); set => Base.CatchType = value.Base; }
    public ExceptionHandlerType HandlerType { get => Base.HandlerType; set => Base.HandlerType = value; }
    
    public ExceptionHandler Clone(ParentInfo info)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return FullName;
    }
}
