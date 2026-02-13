// ------------------------------------------------------------------------
// This file is licensed to you under the MIT License.
// ------------------------------------------------------------------------

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.FluentUI.AspNetCore.McpServer.Models;
using Microsoft.FluentUI.AspNetCore.McpServer.Services;
using ModelContextProtocol.Server;

namespace Microsoft.FluentUI.AspNetCore.McpServer.Resources;

/// <summary>
/// MCP resource providing icon data with SVG content for the explorer UI.
/// </summary>
[McpServerResourceType]
public class IconDataResource
{
    private readonly IconService _iconService;
    private readonly IconSvgProvider _svgProvider;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="IconDataResource"/> class.
    /// </summary>
    public IconDataResource(IconService iconService, IconSvgProvider svgProvider)
    {
        _iconService = iconService;
        _svgProvider = svgProvider;
    }

    /// <summary>
    /// Returns all icons with their metadata and default SVG content.
    /// </summary>
    [McpServerResource(
        UriTemplate = "data://fluent-icons/all.json",
        MimeType = "application/json",
        Title = "Fluent UI Icons Data")]
    [Description("Complete icon catalog with SVG content for the explorer UI")]
    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    public Task<string> GetAllIconsDataAsync()
    {
        var allIcons = _iconService.GetAllIcons();
        var entries = new List<IconExplorerEntry>();

        foreach (var icon in allIcons)
        {
            // Get the default SVG (Regular Size20 preferred)
            var (defaultVariant, defaultSize) = _iconService.GetRecommendedDefault(icon);
            var defaultSvg = _svgProvider.GetSvgContent(icon.Name, defaultVariant, defaultSize);

            entries.Add(new IconExplorerEntry(
                icon.Name,
                icon.VariantNames.ToList(),
                icon.AllSizes.ToList(),
                defaultSvg)
            {
                VariantSizes = icon.Variants
            });
        }

        var response = new IconExplorerDataResponse
        {
            TotalCount = entries.Count,
            Icons = entries
        };

        return Task.FromResult(JsonSerializer.Serialize(response, JsonOptions));
    }

    /// <summary>
    /// Returns SVG content for a specific icon, variant, and size.
    /// </summary>
    [McpServerResource(
        UriTemplate = "data://fluent-icons/{iconName}/{variant}/{size}.svg",
        MimeType = "image/svg+xml",
        Title = "Fluent UI Icon SVG")]
    [Description("Get SVG content for a specific icon variant and size")]
    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    public Task<string> GetIconSvgAsync(string iconName, string variant, int size)
    {
        var svg = _svgProvider.GetSvgContent(iconName, variant, size);

        if (svg is null)
        {
            return Task.FromResult(
                $"<!-- Icon '{iconName}' not found in {variant} Size{size} -->");
        }

        return Task.FromResult(
            $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {size} {size}\" fill=\"currentColor\">{svg}</svg>");
    }

    /// <summary>
    /// Returns all variants with SVG content for a specific icon.
    /// </summary>
    [McpServerResource(
        UriTemplate = "data://fluent-icons/{iconName}/all.json",
        MimeType = "application/json",
        Title = "Fluent UI Icon Variants")]
    [Description("Get all variants and sizes with SVG content for a specific icon")]
    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    public Task<string> GetIconVariantsAsync(string iconName)
    {
        var icon = _iconService.GetIconByName(iconName);

        if (icon is null)
        {
            return Task.FromResult(JsonSerializer.Serialize(new { error = $"Icon '{iconName}' not found" }, JsonOptions));
        }

        var variants = _svgProvider.GetAllVariantsWithSvg(icon);

        return Task.FromResult(JsonSerializer.Serialize(new
        {
            name = icon.Name,
            variants = variants.Select(v => new
            {
                variant = v.Variant,
                size = v.Size,
                svg = v.Svg,
                svgMarkup = v.SvgMarkup,
                code = v.Code
            })
        }, JsonOptions));
    }
}
