using Mono.Cecil.Cil;
using MethodBody = ILWrapper.SubMembers.MethodBody;

namespace ILLoom.Transformers.Injectors;

/// <summary>
/// See <see cref="LoomModLib.Attributes.InjectLocationAttribute"/>
/// </summary>
public interface IInjectLocation
{
    public int ResolveInstructionIndex(MethodBody body);
}

public class InjectHead() : InjectIlIndex(0);
public class InjectIlIndex(int index) : IInjectLocation
{
    public int ResolveInstructionIndex(MethodBody body) => index;
}

public class InjectBeforeReturn(int i) : IInjectLocation
{
    public int ResolveInstructionIndex(MethodBody body)
    {
        // TODO: this will only inject into the final ret instruction since br is used instead of ret
        var index = 0;
        for (var j = 0; j < i; j++)
        {
            while (body.Instructions[index].OpCode != OpCodes.Ret)
            {
                index++;
                if (body.Instructions.Count >= index)
                    throw new IlOffsetResolveException("Return index out of bounds");
            }
        }
        return index;
    }
}


public class IlOffsetResolveException(string message) : Exception(message);