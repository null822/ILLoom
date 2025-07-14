using ILLoom.Transformers.TransformerTypes;
using ILLib.Extensions;
using ILLib.Extensions.Containers;
using ILLib.Extensions.Members;
using LoomModLib.Attributes;
using Mono.Cecil;

namespace ILLoom.Transformers;

public class InjectEnumTransformer(TypeDefinition injector, TypeDefinition target) : ITransformer
{
    private readonly FieldDefinition[] _fields = injector.Fields.ToArray();
    
    public string Name { get; } = $"{injector.FullName} => {target.FullName}";
    
    public void Apply()
    {
        var info = target.get_Info();
        info.Remapper = Program.Remapper;
        
        foreach (var field in _fields)
        {
            if (field.Name == "value__")
                continue;
            
            var existingField = target.Fields.FirstOrDefault(f => f?.Name == field.Name, null);
            if (existingField != null)
            {
                existingField.Constant = field.Constant;
                continue;
            }
            
            var fieldClone = field.Clone(info);
            fieldClone.CustomAttributes.RemoveAll(a => a.AttributeType.Is<DontCopyAttribute>());
            fieldClone.CustomAttributes.RemoveAll(a => a.AttributeType.Is<ForceEnumValueAttribute>());
            fieldClone.FieldType = target;
            
            // find next available constant value, unless we should force the value
            if (!field.CustomAttributes.Any(a => a.AttributeType.Is<ForceEnumValueAttribute>()))
            {
                for (var i = (int)fieldClone.Constant!;; i++)
                {
                    if (target.Fields.Any(f => f.Name != "value__" && (int)(f.Constant ?? -1) == i))
                        continue;
                    
                    fieldClone.Constant = i;
                    break;
                }
            }
            
            target.Fields.Add(fieldClone);
        }
    }
}