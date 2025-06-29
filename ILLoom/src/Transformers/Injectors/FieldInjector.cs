using ILLoom.Transformers.TransformerTypes;
using ILWrapper.Members;

namespace ILLoom.Transformers.Injectors;

public class FieldInjector : IInjector
{
    public string Name => "Field Injector (NYI)";

    public InjectorApplyState Inject() => InjectorApplyState.Failed;
}