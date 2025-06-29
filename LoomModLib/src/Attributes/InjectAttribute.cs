namespace LoomModLib.Attributes;

/// <summary>
/// Inject code into the body of a method or change the default value of a field. When used on a property, requires a
/// <see cref="InjectPropertyLocation"/> attribute as well.
/// </summary>
/// <param name="targetMember">the name of the member to inject into</param>
/// <param name="targetType">the target type the member resides in</param>
[AttributeUsage(AttributeTargets.Method
                | AttributeTargets.Field
                | AttributeTargets.Property, AllowMultiple = true)]
public class InjectAttribute(string targetMember, Type targetType) : Attribute;
