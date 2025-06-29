namespace LoomModLib.Attributes;

/// <summary>
/// Inject into an <see cref="Enum"/>, adding or overriding the values of its fields.
/// </summary>
/// <param name="targetEnum">the <see cref="Enum"/> to inject into</param>
[AttributeUsage(AttributeTargets.Enum, AllowMultiple = true)]
public class InjectEnumAttribute(Type targetEnum) : Attribute;

/// <summary>
/// Forces the value of an enum field in an <see cref="InjectEnumAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class ForceEnumValueAttribute : Attribute;