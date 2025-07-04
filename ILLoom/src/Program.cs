﻿using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using ILLoom.Transformers.TransformerTypes;
using ILWrapper;
using Mono.Cecil;
using Assembly = ILWrapper.Containers.Assembly;
using Module = ILWrapper.Containers.Module;

namespace ILLoom;

public static class Program
{
    public static Module TargetModule => _targetAssembly.MainModule;
    public static ParentInfo TargetInfo { get; private set; }
    public static readonly Remap<MemberReference> Remapper = Remap;
    
    public static readonly AssemblyResolver AssemblyResolver = new();
    public static readonly MetadataResolver MetadataResolver = new(AssemblyResolver);
    
    private static readonly string RootDirectory = Directory.GetCurrentDirectory().Replace('\\', '/');
    private static string _targetPath = "";
    
    private static Mod[] _mods = [];
    
    /// <summary>
    /// A <see cref="Dictionary{TKey,TValue}"/> remapping <see cref="MemberReference"/>s of members with the
    /// <see cref="LoomModLib.Attributes.HoistAttribute"/> or <see cref="LoomModLib.Attributes.HoistTypeAttribute"/>
    /// </summary>
    private static readonly Dictionary<string, MemberReference> HoistRemappings = [];
    
    private static Assembly _targetAssembly = null!;
    
    
    private static readonly ReaderParameters ReaderParameters = new()
    {
        AssemblyResolver = AssemblyResolver,
        MetadataResolver = MetadataResolver
    };
    
    private static System.Reflection.Assembly _patchedAssembly = null!;
    
    private static int Main(string[] args)
    {
        if (args.Length < 1)
            throw new Exception("No target application path supplied");
        
        Directory.CreateDirectory("mods");
        Directory.CreateDirectory("libs");
        _targetPath = args[0].Replace('\\', '/');
        
        AssemblyResolver.RegisterAssemblies(RuntimeEnvironment.GetRuntimeDirectory());
        AssemblyResolver.RegisterAssemblies($"{RootDirectory}/libs");
        
        _targetAssembly = Assembly.ReadAssembly(_targetPath, ReaderParameters);
        AssemblyResolver.RegisterAssembly(_targetAssembly);
        
        TargetInfo = new ParentInfo
        {
            Module = TargetModule,
            Remapper = Remapper
        };
        TargetModule.Info = TargetInfo;
        
        // main script
        Util.Heading("Loading Mods");
        LoadMods();
        Util.Heading("Applying Mods");
        ApplyMods();
        Util.Heading("Build Assembly");
        BuildAssembly();
        Util.Heading("Modloading Complete", '#');
        Console.WriteLine();
        Util.Heading("Launch Assembly");
        LaunchAssembly();
        
        return 0;
    }

    private static void LoadMods()
    {
        var modFiles = Directory.GetFiles($"{RootDirectory}/mods");
        var mods = new List<Mod>(modFiles.Length);
        foreach (var modFileBs in modFiles)
        {
            var modFile = modFileBs.Replace('\\', '/');
            var assembly = Assembly.ReadAssembly(modFile, ReaderParameters);
            var asm = System.Reflection.Assembly.LoadFile(modFile);
            
            Console.WriteLine($"Reading File: {Path.GetFileName(modFile)}");
            foreach (var module in assembly.Modules)
            {
                var mod = new Mod(module, asm);
                mods.Add(mod);
            }
        }
        _mods = mods.ToArray();
        mods.Clear();
    }
    
    private static void ApplyMods()
    {
        Array.ForEach(_mods, m => m.RegisterTypeHoisters());    // REG   [HoistType]
        Array.ForEach(_mods, m => m.RegisterHoisters());        // REG   [Hoist]
        Array.ForEach(_mods, m => m.ScanTypeInserters());       // LOAD  [InsertType]
        ApplyTypeInsertions();                                       // APPLY [InsertType]
        Array.ForEach(_mods, m => m.ScanInserters());           // LOAD  [Insert]
        Array.ForEach(_mods, m => m.ScanEnumInjectors());       // LOAD  [InjectEnum]
        Array.ForEach(_mods, m => m.ScanInjectors());           // LOAD  [Inject]
        Array.ForEach(_mods, m => m.LoadCopyTypes());           // LOAD  [<none>]
        CopyTypes();                                                 // APPLY [<none>]
        ApplyInsertions();                                           // APPLY [Insert]
        ApplyEnumInjectors();                                        // APPLY [InjectEnum]
        ApplyInjectors();                                            // APPLY [Inject]
    }
    
    private static void BuildAssembly()
    {
        Console.WriteLine("Hoist Remappings: ");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        foreach (var remapping in HoistRemappings)
        {
            Console.WriteLine($"  {remapping.Key} => {remapping.Value.FullName}");
        }
        Console.ForegroundColor = ConsoleColor.Gray;
        
        Directory.CreateDirectory("out");
        _targetAssembly.Write("out/patched.dll");
        
        var targetConfig = _targetPath.Remove(_targetPath.LastIndexOf('.')) + ".runtimeconfig.json";
        File.Copy(targetConfig, "out/patched.runtimeconfig.json", true);
        
        // convert ILWrapper.Member.Assembly into System.Reflection.Assembly
        var assemblyStream = new MemoryStream();
        _targetAssembly.Write(assemblyStream);
        _patchedAssembly = System.Reflection.Assembly.Load(assemblyStream.ToArray());
        assemblyStream.Dispose();
        _targetAssembly.Dispose();
        
        Console.WriteLine("Assembly Built Successfully");
    }

    private static void LaunchAssembly()
    {
        // invoke the entrypoint
        
        var action = ApplicationAction.Constructing;
        try
        {
            var entrypoint = _patchedAssembly.EntryPoint ?? throw new Exception("Game assembly does not contain an entrypoint");
            var o = Activator.CreateInstance(entrypoint.ReflectedType ??
                                             throw new Exception("Game assembly does not contain a valid entrypoint"));
            
            string[] applicationArgs = ["hello"];
            action = ApplicationAction.Running;
            
            Console.ForegroundColor = ConsoleColor.White;
            var exitCode = (int)(entrypoint.Invoke(o, [applicationArgs]) ?? 0);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Application Exited with Code {exitCode}");
            
            action = ApplicationAction.Disposing;
            // nothing to dispose
        }
        catch (Exception e)
        {
            if (e is not TargetInvocationException { InnerException: not null } invocationException)
                throw;
            
            var innerException = invocationException.InnerException;
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Exception thrown when {action.ToString().ToLower()} application: {innerException.GetType()}: {innerException.Message}");
            
            var trace = new StackTrace(innerException, true).GetFrames();
            for (var i = 0; i < trace.Length - 2; i++)
            {
                var frame = trace[i];
                Console.WriteLine($"   at {GetMethodSig(frame.GetMethod())} IL_{frame.GetILOffset():X4}");
            }
            
            Console.ForegroundColor = ConsoleColor.White;
        }
        
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    #region ApplyMods Stages

    private static void ApplyTypeInsertions()
    {
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine("  - Insert Types");
        Console.ForegroundColor = ConsoleColor.Gray;
        foreach (var mod in _mods)
        {
            foreach (var typeInserter in mod.TypeInserters)
            {
                Console.WriteLine($"    - {typeInserter.Name}");
                typeInserter.Apply();
                // AddHoistRemap(typeInserter.GetRemapping());
            }
        }
    }
    
    private static void CopyTypes()
    {
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine("  - Copy Types");
        Console.ForegroundColor = ConsoleColor.Gray;
        
        var newNamespace = TargetModule.Entrypoint?.DeclaringType?.GetRootType().Namespace;
        foreach (var mod in _mods)
        {
            foreach (var type in mod.CopyTypes)
            {
                Console.WriteLine($"    - {type.FullName}");
                var clone = type.Clone(TargetInfo);
                if (newNamespace != null)
                    clone.Namespace = newNamespace;
                TargetModule.Types.Add(clone);
            }
        }
    }
    
    private static void ApplyInsertions()
    {
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine("  - Insert Members");
        Console.ForegroundColor = ConsoleColor.Gray;
        foreach (var mod in _mods)
        {
            foreach (var inserter in mod.Inserters)
            {
                Console.WriteLine($"    - {inserter.Name}");
                inserter.Apply();
            }
        }
    }
    
    private static void ApplyEnumInjectors()
    {
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine("  - Inject into Enums");
        Console.ForegroundColor = ConsoleColor.Gray;
        
        foreach (var mod in _mods)
        {
            foreach (var injector in mod.EnumInjectors)
            {
                Console.WriteLine($"    - {injector.Name}");
                injector.Apply();
            }
        }
    }
    
    private static void ApplyInjectors()
    {
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine("  - Inject into Method Bodies");
        Console.ForegroundColor = ConsoleColor.Gray;
        
        foreach (var mod in _mods)
        {
            var injectors = mod.Injectors;
            
            var prevInjectorApplyStates = new InjectorApplyState[injectors.Count];
            var injectorApplyStates = new InjectorApplyState[injectors.Count];
            
            var i = 0;
            while (injectors.Count != 0)
            {
                if (i >= injectors.Count)
                {
                    // if nothing changed in the previous iteration through the injectors, exit
                    if (prevInjectorApplyStates == injectorApplyStates)
                        break;
                    
                    // otherwise, go through the injectors again
                    prevInjectorApplyStates = injectorApplyStates;
                    i = 0;
                }
                
                // if the next injector has already been applied, skip it
                if (injectorApplyStates[i] is InjectorApplyState.Succeeded or InjectorApplyState.Failed)
                {
                    i++;
                    continue;
                }
                
                // run the next injector
                injectorApplyStates[i] = injectors[i].Inject();
                i++;
            }
            
            for (var j = 0; j < injectors.Count; j++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("    [");
                switch (injectorApplyStates[j])
                {
                    case InjectorApplyState.Succeeded:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("  OK  ");
                        break;
                    case InjectorApplyState.MissingDependency:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("DEPEND");
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("FAILED");
                        break;
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"] {injectors[j].Name}");
            }
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    #endregion

    /// <summary>
    /// Register a new hoist remapping.
    /// </summary>
    /// <param name="mapping">the <see cref="HoistRemapping"/></param>
    public static void AddHoistRemap(HoistRemapping mapping)
    {
        HoistRemappings.Add(mapping.From, mapping.To);
    }
    
    /// <summary>
    /// List version of <see cref="AddHoistRemap(HoistRemapping)"/>
    /// </summary>
    /// <param name="mappings">a list of mappings to add</param>
    public static void AddHoistRemappings(List<HoistRemapping> mappings)
    {
        foreach (var mapping in mappings)
        {
            if (!HoistRemappings.TryAdd(mapping.From, mapping.To))
            {
                Console.WriteLine($"Failed to add Hoist remapping: {mapping.From} => {mapping.To.FullName}");
            }
        }
    }
    
    /// <summary>
    /// Remap a <see cref="MemberReference"/> using the <see cref="HoistRemappings"/>
    /// </summary>
    public static MemberReference Remap(MemberReference? reference)
    {
        // if null, return null
        if (reference == null) return reference!;
        
        // if the reference is to a Type, remap it
        if (reference is TypeReference typeReference)
        {
            // create a list of all types in the nested type chain, starting from the deepest
            var typeChain = new List<TypeReference>();
            var type = typeReference;
            while (type != null)
            {
                typeChain.Add(type);
                type = type.DeclaringType;
            }
            
            // try to remap one type in the chain of nested types
            for (var i = 0; i < typeChain.Count; i++)
            {
                if (HoistRemappings.TryGetValue(typeChain[i].FullName, out var remappedType))
                {
                    if (i == 0) return remappedType;
                    
                    typeChain[i - 1].DeclaringType = (TypeReference)remappedType;
                    return reference;
                }
            }
            
            // otherwise, just return the original
            return reference;
        }
        
        // if the reference exists explicitly, remap it
        if (HoistRemappings.TryGetValue(reference.FullName, out var remappedMember))
            return remappedMember;
        
        // otherwise, try to remap the declaring type
        try {
            reference.DeclaringType = (TypeReference)Remap(reference.DeclaringType);
        }
        catch (InvalidOperationException) {}
        
        // return the reference
        return reference;
    }
    
    private static string GetMethodSig(MethodBase? method)
    {
        if (method == null) return "";
        
        var s = new StringBuilder();
        
        s.Append(method.DeclaringType?.FullName);
        s.Append('.');
        s.Append(method.Name);
        s.Append('(');
        var parameters = method.GetParameters();
        if (parameters.Length != 0)
        {
            foreach (var parameter in parameters)
            {
                s.Append(parameter.ParameterType.Name);
                s.Append(' ');
            }
            
            s.Remove(s.Length - 1, 1);
        }
        s.Append(')');
        
        return s.ToString();
    }
    
    private enum ApplicationAction
    {
        Constructing,
        Running,
        Disposing
    }
}

public record HoistRemapping(string From, MemberReference To);
