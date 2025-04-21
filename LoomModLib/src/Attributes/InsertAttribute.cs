namespace LoomModLib.Attributes;

[AttributeUsage(AttributeTargets.Method
                | AttributeTargets.Field
                | AttributeTargets.Property
                | AttributeTargets.Event
                | AttributeTargets.Delegate
                | AttributeTargets.Constructor, AllowMultiple = true)]
public class InsertAttribute : Attribute
{
    public InsertAttribute(string newName, Type parentType) { }
}


[AttributeUsage(AttributeTargets.Class
                | AttributeTargets.Struct
                | AttributeTargets.Interface
                | AttributeTargets.Enum, AllowMultiple = true)]
public class InsertTypeAttribute : Attribute
{
    public InsertTypeAttribute(string assembly, string version, string @namespace,  string targetType) { }
}
