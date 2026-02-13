// ------------------------------------------------------------------------
// This file is licensed to you under the MIT License.
// ------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.McpServer.Models;

namespace Microsoft.FluentUI.AspNetCore.McpServer.Services;

/// <summary>
/// Service for loading SVG content from Fluent UI Icon assemblies.
/// </summary>
public sealed class IconSvgProvider
{
    private const string IconNamespace = "Microsoft.FluentUI.AspNetCore.Components";
    private const string IconAssemblyPattern = "Microsoft.FluentUI.AspNetCore.Components.Icons.{0}";

    private readonly Dictionary<string, Assembly> _loadedAssemblies = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Type[]> _iconTypeCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the SVG content for a specific icon variant and size.
    /// </summary>
    /// <param name="iconName">The icon name (e.g., "Add", "Calendar").</param>
    /// <param name="variant">The variant (e.g., "Filled", "Regular").</param>
    /// <param name="size">The icon size (e.g., 20, 24).</param>
    /// <returns>The SVG path content, or null if the icon is not found.</returns>
    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    public string? GetSvgContent(string iconName, string variant, int size)
    {
        try
        {
            var assembly = GetOrLoadAssembly(variant);
            if (assembly is null)
            {
                return null;
            }

            var iconTypes = GetIconTypes(assembly, variant);
            var iconFullName = $"{IconNamespace}.Icons.{variant}.Size{size}+{iconName}";

            var iconType = iconTypes.FirstOrDefault(t =>
                string.Equals(t.FullName, iconFullName, StringComparison.OrdinalIgnoreCase));

            if (iconType is null)
            {
                return null;
            }

            var iconInstance = Activator.CreateInstance(iconType);
            if (iconInstance is Icon icon)
            {
                return icon.Content;
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Gets an icon instance with SVG content.
    /// </summary>
    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    public IconWithSvg? GetIconWithSvg(IconModel iconModel, string variant, int size)
    {
        var svg = GetSvgContent(iconModel.Name, variant, size);
        if (svg is null)
        {
            return null;
        }

        return new IconWithSvg(
            iconModel.Name,
            variant,
            size,
            svg,
            iconModel.Variants
        );
    }

    /// <summary>
    /// Gets all icon variants with SVG content for a given icon.
    /// </summary>
    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    public IReadOnlyList<IconWithSvg> GetAllVariantsWithSvg(IconModel iconModel)
    {
        var results = new List<IconWithSvg>();

        foreach (var kvp in iconModel.Variants)
        {
            var variant = kvp.Key;
            foreach (var size in kvp.Value)
            {
                var svg = GetSvgContent(iconModel.Name, variant, size);
                if (svg is not null)
                {
                    results.Add(new IconWithSvg(
                        iconModel.Name,
                        variant,
                        size,
                        svg,
                        iconModel.Variants
                    ));
                }
            }
        }

        return results;
    }

    private Assembly? GetOrLoadAssembly(string variant)
    {
        if (_loadedAssemblies.TryGetValue(variant, out var cached))
        {
            return cached;
        }

        var assemblyName = string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            IconAssemblyPattern,
            variant);

        try
        {
            var assembly = Assembly.Load(assemblyName);
            _loadedAssemblies[variant] = assembly;
            return assembly;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private Type[] GetIconTypes(Assembly assembly, string variant)
    {
        var cacheKey = $"{assembly.FullName}_{variant}";

        if (_iconTypeCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var iconTypes = assembly.GetTypes()
            .Where(t => t.BaseType == typeof(Icon))
            .ToArray();

        _iconTypeCache[cacheKey] = iconTypes;
        return iconTypes;
    }
}
