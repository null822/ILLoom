namespace ILLoom.Transformers.TransformerTypes;

public interface IInjector : ITransformer
{
    public InjectorApplyState Inject();
    void ITransformer.Apply() => Inject();
}

public enum InjectorApplyState
{
    Unapplied = 0,
    Succeeded,
    MissingDependency,
    Failed,
}