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
using Zarem.Registry;

namespace Zarem.Serialization;

/// <summary>
/// A class for serializing/deserialization <see cref="IProjectConfig"/> instances.
/// </summary>
public static partial class ProjectSerializer
{
    delegate void DeserializeDelegate(object obj, XElement child, PropertyInfo prop);

    /// <summary>
    /// Loads a <see cref="IProject"/> from XML.
    /// </summary>
    /// <param name="path">The path to the config file.</param>
    /// <returns>The loaded <see cref="IProject"/>.</returns>
    public static IProjectConfig Deserialize(string path)
    {
        var doc = XDocument.Load(path);
        var root = doc.Root!;

        var config = new ProjectConfig();

        ReadObjectProperties(root, config);

        config.ConfigPath = path;
        return config;
    }

    private static void ReadObjectProperties(XElement element, object obj)
    {
        foreach (var child in element.Elements())
        {
            var prop = obj.GetType().GetProperty(child.Name.LocalName);
            if (prop == null)
                continue;

            // Select deserialization function
            // TODO: Dynamically lookup config type
            DeserializeDelegate @delegate = prop switch
            {
                _ when typeof(IArchitectureConfig).IsAssignableFrom(prop.PropertyType) => (obj, child, prop) => DeserializeConfig(ZaremRegistry.Architectures, obj, child, prop),
                _ when typeof(AssemblerConfig).IsAssignableFrom(prop.PropertyType) => (obj, child, prop) => DeserializeConfig(ZaremRegistry.Assemblers, obj, child, prop),
                _ when typeof(EmulatorConfig).IsAssignableFrom(prop.PropertyType) => (obj, child, prop) => DeserializeConfig(ZaremRegistry.Emulators, obj, child, prop),
                _ when typeof(FormatConfig).IsAssignableFrom(prop.PropertyType) => (obj, child, prop) => DeserializeConfig(ZaremRegistry.Formats, obj, child, prop),
                _ when prop.PropertyType.IsEnum => DeserializeEnum,
                _ when IsSimple(prop.PropertyType) => DeserializeSimple,
                _ => DeserializeObject,
            };

            // Run deserialization
            @delegate(obj, child, prop);
        }
    }
    private static void DeserializeConfig<T>(DescriptorRegistry<T> registry, object obj, XElement child, PropertyInfo prop)
        where T : class, IDescriptor
    {
        var identifier = child.Attribute("Type")?.Value;
        Guard.IsNotNull(identifier);

        var descriptor = registry.Get(identifier);
        Guard.IsNotNull(descriptor);

        var config = Activator.CreateInstance(descriptor.ConfigType);
        Guard.IsNotNull(config);

        ReadObjectProperties(child, config);

        prop.SetValue(obj, config);
    }

    private static void DeserializeSimple(object obj, XElement child, PropertyInfo prop)
    {
        var value = Convert.ChangeType(
            child.Value,
            prop.PropertyType);

        prop.SetValue(obj, value);
    }

    private static void DeserializeEnum(object obj, XElement child, PropertyInfo prop)
    {
        var enumValue = ParseXmlEnum(prop.PropertyType, child.Value);
        prop.SetValue(obj, enumValue);
    }

    private static void DeserializeObject(object obj, XElement child, PropertyInfo prop)
    {
        var nestedInstance = Activator.CreateInstance(prop.PropertyType)!;

        ReadObjectProperties(child, nestedInstance);
        prop.SetValue(obj, nestedInstance);
    }

    private static object ParseXmlEnum(Type enumType, string xmlValue)
    {
        foreach (var field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field
                .GetCustomAttributes(typeof(XmlEnumAttribute), false)
                .Cast<XmlEnumAttribute>()
                .FirstOrDefault();

            if (attr != null && attr.Name == xmlValue)
                return field.GetValue(null)!;

            if (field.Name == xmlValue)
                return field.GetValue(null)!;
        }

        throw new InvalidOperationException(
            $"Value '{xmlValue}' is not valid for enum {enumType.Name}");
    }
}
