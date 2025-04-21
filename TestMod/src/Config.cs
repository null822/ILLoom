using LoomModLib;

namespace TestMod;

public class Config : IModConfig
{
    public string Id => SecondaryClass.Id;
    public string[] OptionalDependencies => ["NYI"];
    public string[] RequiredDependencies => ["NYI"];
}
