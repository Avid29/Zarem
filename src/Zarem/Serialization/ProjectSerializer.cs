// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using Zarem.Assembler.Config;
using Zarem.Config;
using Zarem.Descriptors;
using Zarem.Emulator.Config;
using Zarem.Linker.Config;
using Zarem.Registry;

namespace Zarem.Serialization;

/// <summary>
/// A class for serializing/deserialization <see cref="IProjectConfig"/> instances.
/// </summary>
public static partial class ProjectSerializer
{
    delegate void SerializeDelegate(XElement parent, PropertyInfo prop, object value);

    /// <summary>
    /// Serializes a <see cref="IProjectConfig"/>.
    /// </summary>
    /// <param name="config">The <see cref="IProjectConfig"/> to serialize.</param>
    /// <param name="path">The path to store the result in.</param>
    public static void Serialize(IProjectConfig config, string path)
    {
        // Create the root node
        var root = new XElement("Project");

        // Serialize ProjectConfig properties automatically
        WriteObjectProperties(root, config);

        var doc = new XDocument(root);
        doc.Save(path);
    }

    private static void WriteObjectProperties(XElement parent, object obj)
    {
        foreach (var prop in obj.GetType().GetProperties())
        {
            if (!prop.CanRead)
                continue;

            if (Attribute.IsDefined(prop, typeof(XmlIgnoreAttribute)))
                continue;

            var value = prop.GetValue(obj);
            if (value == null)
                continue;

            // Select serialization function
            SerializeDelegate @delegate = prop switch
            {
                _ when value is IArchitectureConfig => (parent, prop, value) => SerializeConfig(ZaremRegistry.Architectures, parent, prop, value),
                _ when value is AssemblerConfig => (parent, prop, value) => SerializeConfig(ZaremRegistry.Assemblers, parent, prop, value),
                _ when value is EmulatorConfig => (parent, prop, value) => SerializeConfig(ZaremRegistry.Emulators, parent, prop, value),
                _ when value is LinkerConfig => (parent, prop, value) => SerializeConfig(ZaremRegistry.Linkers, parent, prop, value),
                _ when value is FormatConfig => (parent, prop, value) => SerializeConfig(ZaremRegistry.Formats, parent, prop, value),
                _ when prop.PropertyType.IsEnum => SerializeEnum,
                _ when IsSimple(prop.PropertyType) => SerializeSimple,
                _ => SerializeObject,
            };

            // Run serialization
            @delegate(parent, prop, value);
        }
    }

    private static void SerializeConfig<T>(DescriptorRegistry<T> registry, XElement parent, PropertyInfo prop, object value)
        where T : class, IDescriptor
    {
        var descriptor = registry.Get(value.GetType());
        Guard.IsNotNull(descriptor);

        var element = new XElement(prop.Name, new XAttribute("Type", descriptor.Identifier));

        WriteObjectProperties(element, value);
        parent.Add(element);
    }

    private static void SerializeSimple(XElement parent, PropertyInfo prop, object value)
    {
        parent.Add(new XElement(prop.Name, value));
    }

    private static void SerializeEnum(XElement parent, PropertyInfo prop, object value)
    {
        var member = prop.PropertyType.GetMember(value.ToString()!)[0];

        var attr = member
            .GetCustomAttributes(typeof(XmlEnumAttribute), false)
            .Cast<XmlEnumAttribute>()
            .FirstOrDefault();

        var name = attr?.Name ?? value.ToString()!;
        parent.Add(new XElement(prop.Name, name));
    }

    private static void SerializeObject(XElement parent, PropertyInfo prop, object value)
    {
        var element = new XElement(prop.Name);
        WriteObjectProperties(element, value);
        parent.Add(element);
    }

    private static bool IsSimple(Type type)
    {
        return type.IsPrimitive
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime);
    }
}
