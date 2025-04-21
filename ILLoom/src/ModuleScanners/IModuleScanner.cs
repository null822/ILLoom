using ILWrapper.Containers;

namespace ILLoom.ModuleScanners;

public interface IModuleScanner<out T>
{
    public T Scan(Module module);
}