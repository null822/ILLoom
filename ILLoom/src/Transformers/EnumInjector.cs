using System.Text;
using LoomModLib.Attributes;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.Transformers;

public class EnumInjector : IInjector
{
    private readonly Type _type;
    private readonly Type?[] _targets;
    
    public string Signature { get; }
    public string Name => _type.FullName;
    
    public EnumInjector(Type injector)
    {
        _type = injector;
        var attributes = injector.CustomAttributes
            .Where(a => a.Type.Is<InjectEnumAttribute>()).ToArray();

        _targets = new Type[attributes.Length];
        var signature = new StringBuilder($"{_type.FullName} -> [");
        
        for (var i = 0; i < attributes.Length; i++)
        {
            var target = attributes[i].ConstructorArguments[0].Value as Type;
            _targets[i] = target;
            signature.Append($"{target}, ");
        }
        signature.Remove(signature.Length - 2, 2);
        Signature = signature.ToString();
        signature.Clear();
    }

}