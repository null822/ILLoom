using Mono.Cecil;
using Module = ILWrapper.Containers.Module;

namespace ILLoom;

public static class Util
{
    public static void Heading(string s, char c = '=', ConsoleColor color = ConsoleColor.Magenta)
    {
        var dashes = Math.Max(0, 13 - s.Length + 15) / 2f + 1;
        var dashesL = (int)Math.Floor(dashes);
        var dashesR = (int)Math.Ceiling(dashes);
        Console.ForegroundColor = color;
        Console.WriteLine($"{"".PadRight(dashesL, c)}[ {s} ]{"".PadRight(dashesR, c)}");
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    public static TypeReference ResolveType(string assemblyName, Version version, string targetSignature, Module relevantModule)
    {
        var separatorIndex = targetSignature.LastIndexOf('.');
        var ns = targetSignature[..separatorIndex];
        var path = targetSignature[(separatorIndex + 1)..].Split('/');
        var targetAssembly = Program.AssemblyResolver.Resolve(new AssemblyNameReference(assemblyName, version));
        var targetRef = new TypeReference(ns, path[0], relevantModule.Base, targetAssembly.MainModule);
        var target = targetRef.Resolve();
        for (var i = 1; i < path.Length; i++)
        {
            target = target.NestedTypes.First(t => t.Name == path[i]);
        }
        
        return target;
    }
    
    public static TypeReference CreateTypeReference(string assemblyName, Version version, string signature)
    {
        var separatorIndex = signature.LastIndexOf('.');
        var @namespace = signature[..separatorIndex];
        var path = signature[(separatorIndex + 1)..].Split('/');
        
        var assembly = Program.AssemblyResolver.Resolve(
            new AssemblyNameReference(assemblyName, version));

        var nestedTypes = new TypeReference[path.Length];
        
        // create a reference to all the types in the chain of nested types the target type is in
        for (var i = 0; i < path.Length; i++)
        {
            var type = new TypeReference(i == 0 ? @namespace : "", path[i], assembly.MainModule, Program.TargetModule.Base);
            nestedTypes[i] = type;
        }
        
        // link up the nested types
        for (var i = 1; i < path.Length; i++)
        {
            nestedTypes[i].DeclaringType = nestedTypes[i - 1];
        }
        
        // return the type at the end of the chain of nested types
        return nestedTypes[^1];
    }
}