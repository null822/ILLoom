using System.Text;
using LoomModLib.Attributes;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.Transformers;

public class EnumInjector : IInjector
{
    public readonly Type Type;
    public readonly Type?[] Targets;
    
    public string Signature { get; }
    
    public EnumInjector(Type injector)
    {
        Type = injector;
        var attributes = injector.CustomAttributes
            .Where(a => a.Type.Is<InjectEnumAttribute>()).ToArray();

        Targets = new Type[attributes.Length];
        var signature = new StringBuilder($"{Type.FullName} -> [");
        
        for (var i = 0; i < attributes.Length; i++)
        {
            var target = attributes[i].ConstructorArguments[0].Value as Type;
            Targets[i] = target;
            signature.Append($"{target}, ");
        }
        signature.Remove(signature.Length - 2, 2);
        Signature = signature.ToString();
        signature.Clear();
    }

}