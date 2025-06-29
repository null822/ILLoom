using ILLoom.Transformers.TransformerTypes;
using ILWrapper;
using ILWrapper.Containers;
using LoomModLib.Attributes;
using Mono.Cecil;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.Transformers;

public class InsertTypeTransformer : ITransformer
{
    private readonly Type _type;
    private readonly Assembly _assembly;
    private readonly string _namespace;
    private readonly string[] _path;
    private readonly string _signature;

    private Type? _endType;

    public string Name => $"{_type.FullName} as {_signature}";

    public InsertTypeTransformer(Type type, Assembly assembly, string signature)
    {
        _type = type;
        _assembly = assembly;
        
        _signature = signature;
        var separatorIndex = signature.LastIndexOf('.');
        _namespace = signature[..separatorIndex];
        _path = signature[(separatorIndex + 1)..].Split('/');
    }
    
    public void Apply()
    {
        // get the root type
        var targetReference = new TypeReference(
            _namespace,
            _path[0],
            _assembly.MainModule.Base,
            _assembly.MainModule.Base);
        var rootTypeDef = Program.MetadataResolver.Resolve(targetReference);
        Type root;
        if (rootTypeDef != null) // if the root type exists, use it
        {
            root = new Type(rootTypeDef);
        }
        else // otherwise, create a new root type and add it to the module
        {
            root = new Type(_namespace, _path[0], TypeAttributes.Class ^ TypeAttributes.Public);
            _assembly.MainModule.Types.Add(root);
        }
        
        // create an array for the nested types
        var nestedTypes = new Type[_path.Length];
        
        // add the root type to the nested types
        nestedTypes[0] = root;
        
        // create or retrieve the nested types
        for (var i = 1; i < _path.Length - 1; i++)
        {
            // get the parent
            var parent = nestedTypes[i - 1];
            
            // get from the parent, or create if it doesn't exist, the next type, and add it to the nested types
            var type = parent.NestedTypes.FirstOrDefault(t => t != null && t.Name == _path[i], null);
            if (type == null)
            {
                // note that the namespace must be an empty string for the nested types to be added to the assembly
                type = new Type("", _path[i], TypeAttributes.Class | TypeAttributes.Public);
                if (i != 0)
                {
                    type.Attributes |= TypeAttributes.NestedPublic | TypeAttributes.NestedPrivate;
                }
            }
            nestedTypes[i] = type;
        }
        
        // remap the name of the type to insert and add it to the nested types
        _endType = _type.Clone(Program.TargetInfo.With(_assembly.MainModule));
        _endType.CustomAttributes.RemoveAll(a => a.Type.Is<DontCopyAttribute>());
        _endType.Name = _path[^1];
        _endType.DeclaringType = null;
        nestedTypes[^1] = _endType;
        
        // link up the nested types
        for (var i = 0; i < _path.Length - 1; i++)
        {
            nestedTypes[i].NestedTypes.Add(nestedTypes[i + 1]);
        }
        
        // the root type is already inserted into the module, so nothing more needs to be done
    }
}
