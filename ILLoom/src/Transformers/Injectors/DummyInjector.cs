using ILLoom.Transformers.TransformerTypes;
using ILWrapper.Members;

namespace ILLoom.Transformers.Injectors;

public class DummyInjector : IInjector
{
    public string Name => "Dummy";

    public InjectorApplyState Inject() => InjectorApplyState.Succeeded;
}