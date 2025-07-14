using ILLoom.ModuleScanners.ScannerTypes;
using ILLoom.Transformers.Injectors;
using ILLoom.Transformers.TransformerTypes;
using ILLib.Extensions;
using ILLib.Extensions.Containers;
using ILLib.Extensions.SubMembers;
using LoomModLib.Attributes;
using Mono.Cecil;

namespace ILLoom.ModuleScanners;

public class InjectScanner : ModuleMemberScanner<IInjector>
{
    protected override IInjector ReadAttribute(CustomAttribute attribute, IMemberDefinition owner)
    {
        //TODO: resolve injector dependencies
        
        
        if (owner is FieldDefinition)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"TODO: Field Injector ({owner})");
            Console.ForegroundColor = ConsoleColor.Gray;
            
            return new FieldInjector();
        }
        
        // resolve inject locations
        var locationAttribs = owner.CustomAttributes
            .Where(a => a.AttributeType.Resolve().Extends<InjectLocationAttribute>())
            .ToArray();
        if (locationAttribs.Length == 0)
            throw new MissingInjectLocationException(owner);
        var locations = new IInjectLocation[locationAttribs.Length];
        for (var i = 0; i < locationAttribs.Length; i++)
        {
            var attrib = locationAttribs[i];
            
            if (attrib.AttributeType.Is<InjectHeadAttribute>())
                locations[i] = new InjectHead();
            else if (attrib.AttributeType.Is<InjectIlIndexAttribute>())
                locations[i] = new InjectIlIndex(attrib.ConstructorArguments[0].As<int>());
            else if (attrib.AttributeType.Is<InjectBeforeReturnAttribute>())
                locations[i] = new InjectBeforeReturn(attrib.ConstructorArguments[0].As<int>());
        }
        
        
        // resolve inject method and target
        var targetMember = attribute.Get<string>(0);
        var targetType = Program.Remap(attribute.Get<TypeReference?>(1) ?? owner.DeclaringType).Resolve();
        
        MethodDefinition method;
        MethodDefinition target;
        
        if (owner is PropertyDefinition p)
        {
            var propertyMethodAttrib = p.CustomAttributes
                .FirstOrDefault(a => a!.AttributeType.Is<InjectPropertyLocation>(), null);
            if (propertyMethodAttrib == null)
                throw new MissingInjectPropertyLocationException(p);
            var propertyMethod = propertyMethodAttrib.Get<PropertyMethod>(0);
            method = propertyMethod switch
            {
                PropertyMethod.Getter => p.GetMethod,
                PropertyMethod.Setter => p.SetMethod,
            };
            
            var targetProperty = targetType.Properties
                .First(m => m.Name == targetMember);
            target = propertyMethod switch
            {
                PropertyMethod.Getter => targetProperty.GetMethod,
                PropertyMethod.Setter => targetProperty.SetMethod!,
            };
        }
        else
        {
            method = (MethodDefinition)owner;
            target = targetType.Methods
                .First(m => m.Name == targetMember && m.Parameters.Matches(method.Parameters));
        }
        
        return new MethodInjector(method, target, locations);
    }
    
    protected override bool IncludeAttribute(CustomAttribute attribute)
    {
        return attribute.AttributeType.Is<InjectAttribute>();
    }

    protected override bool RemoveTransformer(CustomAttribute attribute)
    {
        return true;
    }
}

public class MissingInjectLocationException(IMemberDefinition owner)
    : Exception($"Injector {owner.FullName} is missing an InjectLocation");
public class MissingInjectPropertyLocationException(PropertyDefinition owner)
    : Exception($"Property Injector {owner.FullName} is missing an InjectPropertyLocation");
    