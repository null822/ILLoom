namespace LoomModLib.Attributes;

[AttributeUsage(AttributeTargets.Method
                | AttributeTargets.Field
                | AttributeTargets.Property, AllowMultiple = true)]
public abstract class InjectLocationAttribute : Attribute;

/// <summary>
/// Inject code into the specified method in a property. Required when injecting into a property.
/// </summary>
/// <param name="method"></param>
public class InjectPropertyLocation(PropertyMethod method) : InjectLocationAttribute;

public enum PropertyMethod
{
    Getter,
    Setter
}

/// <summary>
/// Inject code at the start of the target method.
/// </summary>
public class InjectHeadAttribute : InjectLocationAttribute;

/// <summary>
/// Inject code before a specific IL instruction in the method.
/// </summary>
/// <param name="offset">the index of the IL instruction to insert the code before</param>
public class InjectIlIndexAttribute(int offset) : InjectLocationAttribute;

/// <summary>
/// Inject code before a return statement.
/// </summary>
/// <param name="returnIndex">the index of the return statement.
/// May be negative to count from the end (-1 = the last return)</param>
public class InjectBeforeReturnAttribute(int returnIndex) : InjectLocationAttribute;

