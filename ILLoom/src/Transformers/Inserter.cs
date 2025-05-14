using ILWrapper;
using ILWrapper.Members;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.Transformers;

public class Inserter : ITransformer
{
    private readonly Type _type;
    private readonly IMember _member;
    private readonly string _newName;

    public Inserter(Type type, IMember member, string newName)
    {
        _type = type;
        _member = member;
        _newName = newName;
    }
    
    public void Apply()
    {
        var parentInfo = _type.Info;
        parentInfo.Remapper = Program.Remapper;
        
        switch (_member)
        {
            case Event m:
                var @event = m.Clone(parentInfo);
                @event.Name = _newName;
                @event.DeclaringType = null!;
                _type.Events.Add(@event);
                break;
            case Field m:
                var field = m.Clone(parentInfo);
                field.Name = _newName;
                field.DeclaringType = null!;
                _type.Fields.Add(field);
                break;
            case Method m:
                var method = m.Clone(parentInfo);
                method.Name = _newName;
                method.DeclaringType = null!;
                _type.Methods.Add(method);
                break;
            case Property m:
                var property = m.Clone(parentInfo);
                property.Name = _newName;
                property.DeclaringType = null!;
                _type.Properties.Add(property);
                break;
            case Type m:
                var type = m.Clone(parentInfo);
                type.Name = _newName;
                type.DeclaringType = null!;
                _type.NestedTypes.Add(type);
                break;
        }
    }
}