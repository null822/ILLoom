using ILLoom.Transformers.TransformerTypes;

namespace ILLoom.Transformers.Injectors;

public class FieldInjector : IInjector
{
    public string Name => "Field Injector (NYI)";

    public InjectorApplyState Inject() => InjectorApplyState.Failed;
}