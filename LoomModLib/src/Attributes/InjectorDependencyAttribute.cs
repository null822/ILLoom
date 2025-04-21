namespace LoomModLib.Attributes;

[AttributeUsage(AttributeTargets.All
                ^ AttributeTargets.Assembly
                ^ AttributeTargets.Module
                ^ AttributeTargets.Class
                ^ AttributeTargets.Struct
                ^ AttributeTargets.Interface
                ^ AttributeTargets.ReturnValue
                ^ AttributeTargets.Parameter
                ^ AttributeTargets.GenericParameter, AllowMultiple = true)]
public class InjectorDependencyAttribute : Attribute
{
    public InjectorDependencyAttribute(string injector, string injectorClass = "<self>", string modId = "<self>",
        bool invert = false,
        bool optional = false
        ) { }
}
