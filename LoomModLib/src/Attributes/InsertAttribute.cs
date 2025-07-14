namespace LoomModLib.Attributes;

/// <summary>
/// Insert this type into the specified location.
/// </summary>
/// <param name="assembly">the name of the assembly to insert into</param>
/// <param name="targetType">the new name of the type once inserted. Can be a nested type; containing classes will be
/// auto-generated</param>
/// <param name="ns">the namespace to insert the type into. Will default to the assembly name if not set</param>
/// <param name="assemblyVersion">the version of the assembly to insert into</param>
[AttributeUsage(AttributeTargets.Class
                | AttributeTargets.Struct
                | AttributeTargets.Interface
                | AttributeTargets.Enum, AllowMultiple = true)]
public class InsertTypeAttribute(string assembly, string targetType, string ns = "<asm_name>", string assemblyVersion = "*") : Attribute;

/// <summary>
/// Insert this member into the specified location.
/// </summary>
/// <param name="newName">the new name of the member once inserted</param>
/// <param name="parentType">the type to insert this member into</param>
[AttributeUsage(AttributeTargets.Method
                | AttributeTargets.Field
                | AttributeTargets.Property
                | AttributeTargets.Event
                | AttributeTargets.Delegate
                | AttributeTargets.Constructor, AllowMultiple = true)]
public class InsertAttribute(string newName, Type parentType) : Attribute;
