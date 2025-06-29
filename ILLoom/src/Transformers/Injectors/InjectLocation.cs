using Mono.Cecil.Cil;
using MethodBody = ILWrapper.SubMembers.MethodBody;

namespace ILLoom.Transformers.Injectors;

/// <summary>
/// See <see cref="LoomModLib.Attributes.InjectLocationAttribute"/>
/// </summary>
public interface IInjectLocation
{
    public int ResolveIlOffset(MethodBody body);
}

public class InjectHead : IInjectLocation
{
    public int ResolveIlOffset(MethodBody body) => 0;
}

public class InjectBeforeReturn(int i) : IInjectLocation
{
    public int ResolveIlOffset(MethodBody body)
    {
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