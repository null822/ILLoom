using ILLoom.Transformers.TransformerTypes;
using ILWrapper.Members;
using LoomModLib.Attributes;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.Transformers;

public class InjectEnumTransformer(Type injector, Type target) : ITransformer
{
    private readonly Field[] _fields = injector.Fields.ToArray();

    public string Name { get; } = $"{injector.FullName} => {target.FullName}";

    public void Apply()
    {
        var info = target.Info;
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
            fieldClone.CustomAttributes.RemoveAll(a => a.Type.Is<DontCopyAttribute>());
            fieldClone.CustomAttributes.RemoveAll(a => a.Type.Is<ForceEnumValueAttribute>());
            fieldClone.FieldType = target;
            
            // find next available constant value, unless we should force the value
            if (!field.CustomAttributes.Any(a => a.Type.Is<ForceEnumValueAttribute>()))
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