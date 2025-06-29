using ILLoom.ModuleScanners.ScannerTypes;
using ILLoom.Transformers.Injectors;
using ILLoom.Transformers.TransformerTypes;
using ILWrapper;
using ILWrapper.Members;
using LoomModLib.Attributes;
using Mono.Cecil;
using CustomAttribute = ILWrapper.SubMembers.CustomAttribute;
using Type = ILWrapper.Containers.Type;

namespace ILLoom.ModuleScanners;

public class InjectScanner : ModuleMemberScanner<IInjector>
{
    protected override IInjector ReadAttribute(CustomAttribute attribute, IMember owner)
    {
        //TODO: resolve injector dependencies
        
        
        if (owner is Field)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"TODO: Field Injector ({owner})");
            Console.ForegroundColor = ConsoleColor.Gray;
            
            return new FieldInjector();
        }
        
        // resolve inject locations
        var locationAttribs = owner.CustomAttributes
            .Where(a => a.Type.Extends<InjectLocationAttribute>())
            .ToArray();
        if (locationAttribs.Length == 0)
            throw new MissingInjectLocationException(owner);
        var locations = new IInjectLocation[locationAttribs.Length];
        for (var i = 0; i < locationAttribs.Length; i++)
        {
            var attrib = locationAttribs[i];
            
            if (attrib.Type.Is<InjectHeadAttribute>())
                locations[i] = new InjectHead();
            else if (attrib.Type.Is<InjectBeforeReturnAttribute>())
                locations[i] = new InjectBeforeReturn(attrib.ConstructorArguments[0].As<int>());
        }
        
        
        // resolve inject method and target
        var targetMember = attribute.Get<string>(0);
        var targetType = new Type(
            ((TypeReference)Program.Remap(attribute.Get<TypeReference>(1))).Resolve());
        
        Method method;
        Method target;
        
        if (owner is Property p)
        {
            var propertyMethodAttrib = p.CustomAttributes
                .FirstOrDefault(a => a!.Type.Is<InjectPropertyLocation>(), null);
            if (propertyMethodAttrib == null)
                throw new MissingInjectPropertyLocationException(p);
            var propertyMethod = propertyMethodAttrib.Get<PropertyMethod>(0);
            method = propertyMethod switch
            {
                PropertyMethod.Getter => p.Getter!,
                PropertyMethod.Setter => p.Setter!,
            };
            
            var targetProperty = targetType.Properties
                .First(m => m.Name == targetMember);
            target = propertyMethod switch
            {
                PropertyMethod.Getter => targetProperty.Getter!,
                PropertyMethod.Setter => targetProperty.Setter!,
            };
        }
        else
        {
            method = (Method)owner;
            target = targetType.Methods
                .First(m => m.Name == targetMember);
        }
        
        return new MethodInjector(method, target, locations);
    }
    
    protected override bool IncludeAttribute(CustomAttribute attribute)
    {
        return attribute.Type.Is<InjectAttribute>();
    }

    protected override bool RemoveTransformer(CustomAttribute attribute)
    {
        return true;
    }
}

public class MissingInjectLocationException(IMember owner)
    : Exception($"Injector {owner.MemberBase.FullName} is missing an InjectLocation");
public class MissingInjectPropertyLocationException(Property owner)
    : Exception($"Property Injector {owner.MemberBase.FullName} is missing an InjectPropertyLocation");
    