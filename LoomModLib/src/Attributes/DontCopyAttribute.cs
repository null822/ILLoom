namespace LoomModLib.Attributes;

/// <summary>
/// Do not copy this member/type into the target assembly.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class DontCopyAttribute : Attribute;
