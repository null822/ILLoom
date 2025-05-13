namespace LoomModLib.Attributes;

[AttributeUsage(AttributeTargets.Method
                | AttributeTargets.Field
                | AttributeTargets.Property
                | AttributeTargets.Event
                | AttributeTargets.Delegate
                | AttributeTargets.Constructor)]
public class HoistAttribute : Attribute
{
    public HoistAttribute(string targetMember, Type? targetType = null) { }
}

[AttributeUsage(AttributeTargets.Class
                | AttributeTargets.Struct
                | AttributeTargets.Interface
                | AttributeTargets.Enum)]
public class HoistTypeAttribute : Attribute
{
    public HoistTypeAttribute(string assembly, string version,  string targetType) { }
}
