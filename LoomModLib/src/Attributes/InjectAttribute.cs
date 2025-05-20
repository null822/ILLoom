namespace LoomModLib.Attributes;

[AttributeUsage(AttributeTargets.Method
                | AttributeTargets.Field
                | AttributeTargets.Property, AllowMultiple = true)]
public class InjectAttribute(string targetMember, Type targetType) : Attribute;



public interface IInjectLocation;

[AttributeUsage(AttributeTargets.Method
                | AttributeTargets.Field
                | AttributeTargets.Property)]
public class InjectStart : Attribute, IInjectLocation;