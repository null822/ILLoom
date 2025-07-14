using ILLib.Extensions.Containers;
using Mono.Cecil;

namespace ILLib.Extensions.Members;

public static class EventExtensions
{
    public static EventDefinition Clone(this EventDefinition self, ParentInfo info)
    {
        var clone = new EventDefinition(self.Name, self.Attributes, self.EventType.RemapAndImport(info))
        {
            DeclaringType = info.Type?.Resolve() ?? self.DeclaringType,
            AddMethod = self.AddMethod.Clone(info),
            InvokeMethod = self.InvokeMethod.Clone(info),
            RemoveMethod = self.RemoveMethod.Clone(info)
        };
        info.Event = clone;
        clone.CustomAttributes.ReplaceContents(self.CustomAttributes, info);
        clone.OtherMethods.ReplaceContents(self.OtherMethods, info);
        
        return clone;
    }
}