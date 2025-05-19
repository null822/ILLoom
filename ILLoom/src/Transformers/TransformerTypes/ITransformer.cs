namespace ILLoom.Transformers.TransformerTypes;

public interface ITransformer
{
    public string Name { get; }
    public void Apply();
}