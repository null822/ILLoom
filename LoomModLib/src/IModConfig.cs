namespace LoomModLib;

public interface IModConfig
{
    public string Id { get; }
    
    public string[] OptionalDependencies { get; }
    public string[] RequiredDependencies { get; }
}
