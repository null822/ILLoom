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
    public InjectorDependencyAttribute(string mixin, string mixinClass = "<self>", string modId = "<self>",
        bool invert = false,
        bool optional = false
        ) { }
}
