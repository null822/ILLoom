namespace LoomModLib.Attributes;

[AttributeUsage(AttributeTargets.Method 
                | AttributeTargets.Field
                | AttributeTargets.Property, AllowMultiple = true)]
public class InjectAttribute : Attribute
{
    public InjectAttribute(string targetMember, Type targetType) { }
}

[AttributeUsage(AttributeTargets.Enum, AllowMultiple = true)]
public class InjectEnumAttribute : Attribute
{
    public InjectEnumAttribute(Type targetEnum) { }
}
