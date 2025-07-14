namespace LoomModLib.Attributes;

/// <summary>
/// Remap all references to this type to the specified type.
/// </summary>
/// <param name="assembly">the name of the assembly to remap to</param>
/// <param name="targetType">the name of the type to remap to. Can be a nested type</param>
/// <param name="ns">the namespace of the type to remap to. Will default to the assembly name if not set</param>
/// <param name="assemblyVersion">the version of the assembly to remap to</param>
[AttributeUsage(AttributeTargets.Class
                | AttributeTargets.Struct
                | AttributeTargets.Interface
                | AttributeTargets.Enum)]
public class HoistTypeAttribute(string assembly, string targetType, string ns = "<asm_name>", string assemblyVersion = "*") : Attribute;


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
