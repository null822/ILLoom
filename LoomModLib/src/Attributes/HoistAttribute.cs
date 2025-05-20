namespace LoomModLib.Attributes;

/// <summary>
/// Remap all references to this type to the specified type.
/// </summary>
/// <param name="assembly">the name of the assembly to remap to</param>
/// <param name="version">the version of the assembly to remap to</param>
/// <param name="targetType">the name of the type to remap to. Can be a nested type</param>
[AttributeUsage(AttributeTargets.Class
                | AttributeTargets.Struct
                | AttributeTargets.Interface
                | AttributeTargets.Enum)]
public class HoistTypeAttribute(string assembly, string version,  string targetType) : Attribute;


/// <summary>
/// Remap all references to this member to the specified member.
/// </summary>
/// <param name="targetMember">the name of the targeted member</param>
/// <param name="targetType">the type the targeted member resides in</param>
[AttributeUsage(AttributeTargets.Method
                | AttributeTargets.Field
                | AttributeTargets.Property
                | AttributeTargets.Event
                | AttributeTargets.Delegate
                | AttributeTargets.Constructor)]
public class HoistAttribute(string targetMember, Type? targetType = null) : Attribute;
