using ILLoom.Transformers.TransformerTypes;

namespace ILLoom.Transformers.Injectors;

public class DummyInjector : IInjector
{
    public string Name => "Dummy";

    public InjectorApplyState Inject() => InjectorApplyState.Succeeded;
}