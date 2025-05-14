
using ILWrapper.MemberSet;
using Mono.Cecil;
using Type = ILWrapper.Containers.Type;

namespace ILWrapper.Members;

public class Event : IMember<Event, EventDefinition>, IMember
{
    public EventDefinition Base { get; }
    public MemberReference MemberBase => Base;
    static TypeConvert<EventDefinition, Event> IMember<Event, EventDefinition>.FromBase => instance => new Event(instance);
    
    public ParentInfo Info { get; }
    
    public Event(EventDefinition @base)
    {
        Base = @base;
        CustomAttributes = new MemberSet<SubMembers.CustomAttribute, CustomAttribute>(Base.Resolve().CustomAttributes);
        OtherMethods = MemberSet<Method, MethodReference>.From(Base.Resolve().OtherMethods);
        
        Info = new ParentInfo();
    }
    
    public Event(string name, EventAttributes attributes, Type type) : this(new EventDefinition(name, attributes, type.Base)) {}
    
    public string Name { get => Base.Name; set => Base.Name = value; }
    public string FullName => Base.FullName;
    
    public Method AddMethod { get => new(Base.AddMethod); set => Base.AddMethod = value.Base.Resolve(); }
    public Method InvokeMethod { get => new(Base.InvokeMethod); set => Base.InvokeMethod = value.Base.Resolve(); }
    public Method RemoveMethod { get => new(Base.RemoveMethod); set => Base.RemoveMethod = value.Base.Resolve(); }
    
    public Type EventType { get => new(Base.EventType); set => Base.EventType = value.Base.Resolve(); }
    
    public Type? DeclaringType { get => IMember<Type, TypeReference>.Create(Base.DeclaringType); set => Base.DeclaringType = value?.Base.Resolve(); }
    public EventAttributes Attributes { get => Base.Resolve().Attributes; set => Base.Resolve().Attributes = value; }
    
    public IMemberSet<SubMembers.CustomAttribute> CustomAttributes { get; }
    public IMemberSet<Method> OtherMethods { get; }
    
    public Event Clone(ParentInfo info)
    {
        var clone = new Event(Name, Attributes, info.Remap(EventType))
        {
            AddMethod = AddMethod.Clone(info),
            InvokeMethod = InvokeMethod.Clone(info),
            RemoveMethod = RemoveMethod.Clone(info),
            DeclaringType = info.Type
        };
        info.Event = clone;
        clone.OtherMethods.ReplaceContents(OtherMethods, info);
        
        return clone;
    }
}
