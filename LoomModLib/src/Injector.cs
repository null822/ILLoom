namespace LoomModLib;

public static class Injector
{
    /// <summary>
    /// Inserts an early return in the target method that returns the value <paramref name="v"/>.
    /// </summary>
    public static void Return<T>(T? value = default) {}
    public static void Return() {}
}