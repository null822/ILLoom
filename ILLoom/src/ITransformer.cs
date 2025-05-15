namespace ILLoom;

public interface ITransformer
{
    public string Name { get; }
    public void Apply();
}