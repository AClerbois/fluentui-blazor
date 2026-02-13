// ------------------------------------------------------------------------
// This file is licensed to you under the MIT License.
// ------------------------------------------------------------------------

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Web;
using Microsoft.FluentUI.AspNetCore.McpServer.Models;
using Microsoft.FluentUI.AspNetCore.McpServer.Services;
using ModelContextProtocol.Server;

namespace Microsoft.FluentUI.AspNetCore.McpServer.Tools;

/// <summary>
/// MCP tools for searching and discovering Fluent UI Blazor icons.
/// </summary>
[McpServerToolType]
public class IconTools
{
    private const int MaxResults = 50;
    private const int MaxResultsWithPreview = 20;
    private readonly IconService _iconService;
    private readonly IconSvgProvider _svgProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="IconTools"/> class.
    /// </summary>
    public IconTools(IconService iconService, IconSvgProvider svgProvider)
    {
        _iconService = iconService;
        _svgProvider = svgProvider;
    }

    /// <summary>
    /// Searches for Fluent UI icons by name or keyword.
    /// Supports common synonyms (e.g., 'trash' finds Delete, 'notification' finds Alert).
    /// </summary>
    [McpServerTool]
    [Description("Search for Fluent UI icons by name or keyword. Supports synonyms like 'trash' → Delete, 'notification' → Alert, 'gear' → Settings. Use this to find icons for buttons, menus, toolbars, navigation, and other UI elements.")]
    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    public string SearchIcons(
        [Description("The search term: an icon name (e.g., 'Bookmark') or a keyword (e.g., 'trash', 'notification', 'settings', 'calendar').")]
        string searchTerm,
        [Description("Optional: Filter by variant — 'Filled' (solid), 'Regular' (outlined), 'Light' (thin, size 32 only), or 'Color' (multi-color).")]
        string? variant = null,
        [Description("Optional: Filter by size — 10, 12, 16, 20, 24, 28, 32, or 48.")]
        int? size = null)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return "Please provide a search term (e.g., 'calendar', 'settings', 'delete').";
        }

        var results = _iconService.SearchIcons(searchTerm, variant, size);

        if (results.Count == 0)
        {
            return FormatNoResults(searchTerm, variant, size);
        }

        return FormatSearchResults(results, variant, size);
    }

    private static string FormatNoResults(string searchTerm, string? variant, int? size)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"No icons found matching '{searchTerm}'.");
        if (!string.IsNullOrEmpty(variant))
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"  Variant filter: {variant}");
        }

        if (size.HasValue)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"  Size filter: {size.Value}");
        }

        sb.AppendLine();
        sb.AppendLine("Tips:");
        sb.AppendLine("- Try a broader keyword (e.g., 'arrow' instead of 'arrow-left')");
        sb.AppendLine("- Use common terms like 'trash', 'notification', 'gear', 'home'");
        sb.AppendLine("- Remove the variant/size filter to see all matches");
        return sb.ToString();
    }

    private string FormatSearchResults(IReadOnlyList<IconModel> results, string? variant, int? size)
    {
        var sb = new StringBuilder();
        var showPreview = results.Count <= MaxResultsWithPreview;
        var maxToShow = showPreview ? MaxResultsWithPreview : MaxResults;

        sb.AppendLine(CultureInfo.InvariantCulture, $"# Icon Search Results ({results.Count} found{(results.Count > maxToShow ? $", showing first {maxToShow}" : "")})");
        sb.AppendLine();

        AppendFilters(sb, variant, size);

        if (showPreview)
        {
            // Show visual preview with SVG icons
            sb.AppendLine("| Preview | Icon Name | Variants | Sizes | Code Example |");
            sb.AppendLine("|---------|-----------|----------|-------|--------------|");

            foreach (var icon in results.Take(maxToShow))
            {
                var variants = string.Join(", ", icon.VariantNames);
                var sizes = string.Join(", ", icon.AllSizes);
                var (defaultVariant, defaultSize) = _iconService.GetRecommendedDefault(icon);
                var code = $"`new Icons.{defaultVariant}.Size{defaultSize}.{icon.Name}()`";
                var preview = GetSvgPreview(icon.Name, defaultVariant, defaultSize);
                sb.AppendLine(CultureInfo.InvariantCulture, $"| {preview} | **{icon.Name}** | {variants} | {sizes} | {code} |");
            }
        }
        else
        {
            // Text-only table for larger result sets
            sb.AppendLine("| Icon Name | Variants | Sizes | Code Example |");
            sb.AppendLine("|-----------|----------|-------|--------------|");

            foreach (var icon in results.Take(maxToShow))
            {
                var variants = string.Join(", ", icon.VariantNames);
                var sizes = string.Join(", ", icon.AllSizes);
                var (defaultVariant, defaultSize) = _iconService.GetRecommendedDefault(icon);
                var code = $"`new Icons.{defaultVariant}.Size{defaultSize}.{icon.Name}()`";
                sb.AppendLine(CultureInfo.InvariantCulture, $"| **{icon.Name}** | {variants} | {sizes} | {code} |");
            }
        }

        if (results.Count > maxToShow)
        {
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"*{results.Count - maxToShow} more results not shown. Refine your search or add variant/size filters.*");
        }

        sb.AppendLine();
        sb.AppendLine("**Usage:** `<FluentIcon Value=\"@(new Icons.{Variant}.Size{Size}.{Name}())\" />`");

        return sb.ToString();
    }

    /// <summary>
    /// Gets an SVG preview as a markdown image.
    /// </summary>
    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    private string GetSvgPreview(string iconName, string variant, int size)
    {
        var svgContent = _svgProvider.GetSvgContent(iconName, variant, size);
        if (string.IsNullOrEmpty(svgContent))
        {
            return "❓";
        }

        // Create a complete SVG with viewBox and currentColor
        var fullSvg = $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {size} {size}\" width=\"20\" height=\"20\" fill=\"currentColor\">{svgContent}</svg>";

        // Encode for markdown image (data URI)
        var encodedSvg = HttpUtility.UrlEncode(fullSvg).Replace("+", "%20", StringComparison.Ordinal);
        return $"![{iconName}](data:image/svg+xml,{encodedSvg})";
    }

    private static void AppendFilters(StringBuilder sb, string? variant, int? size)
    {
        if (string.IsNullOrEmpty(variant) && !size.HasValue)
        {
            return;
        }

        sb.Append("**Filters:** ");
        if (!string.IsNullOrEmpty(variant))
        {
            sb.Append(CultureInfo.InvariantCulture, $"variant={variant} ");
        }

        if (size.HasValue)
        {
            sb.Append(CultureInfo.InvariantCulture, $"size={size.Value}");
        }

        sb.AppendLine();
        sb.AppendLine();
    }

    /// <summary>
    /// Gets full details for a specific Fluent UI icon including all available variants and sizes.
    /// </summary>
    [McpServerTool]
    [Description("Get full details for a specific Fluent UI icon including all available variants (Filled, Regular, Light, Color) and sizes. Use this after SearchIcons to get complete information about an icon.")]
    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    public string GetIconDetails(
        [Description("The exact icon name (e.g., 'Bookmark', 'Alert', 'Calendar', 'ArrowLeft').")]
        string iconName)
    {
        if (string.IsNullOrWhiteSpace(iconName))
        {
            return "Please provide an icon name.";
        }

        var icon = _iconService.GetIconByName(iconName);
        if (icon is null)
        {
            return FormatIconNotFound(iconName);
        }

        return FormatIconDetails(icon);
    }

    /// <summary>
    /// Shows a single Fluent UI icon with a large visual preview.
    /// </summary>
    [McpServerTool]
    [Description("Display a single Fluent UI icon with a large visual preview. Shows the icon in the specified variant and size, with the Blazor code to use it.")]
    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    public string ShowIcon(
        [Description("The exact icon name (e.g., 'Bookmark', 'Alert', 'Calendar', 'DocumentEdit').")]
        string iconName,
        [Description("Optional: The variant — 'Filled' (solid), 'Regular' (outlined), 'Light' (thin). Defaults to Regular.")]
        string? variant = null,
        [Description("Optional: The size — 10, 12, 16, 20, 24, 28, 32, or 48. Defaults to 20.")]
        int? size = null)
    {
        if (string.IsNullOrWhiteSpace(iconName))
        {
            return "Please provide an icon name.";
        }

        var icon = _iconService.GetIconByName(iconName);
        if (icon is null)
        {
            return FormatIconNotFound(iconName);
        }

        // Determine variant and size
        var selectedVariant = variant ?? "Regular";
        if (!icon.Variants.ContainsKey(selectedVariant))
        {
            selectedVariant = icon.VariantNames.FirstOrDefault() ?? "Regular";
        }

        var availableSizes = icon.Variants.TryGetValue(selectedVariant, out var sizes) ? sizes : icon.AllSizes;
        var selectedSize = size ?? (availableSizes.Contains(20) ? 20 : availableSizes.FirstOrDefault());

        if (!availableSizes.Contains(selectedSize))
        {
            selectedSize = availableSizes.FirstOrDefault();
        }

        return FormatSingleIcon(icon, selectedVariant, selectedSize);
    }

    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    private string FormatSingleIcon(IconModel icon, string variant, int size)
    {
        var sb = new StringBuilder();

        // Large header with icon name
        sb.AppendLine(CultureInfo.InvariantCulture, $"# {icon.Name}");
        sb.AppendLine();

        // Large visual preview
        AppendSingleIconPreview(sb, icon, variant, size);

        // Info
        AppendSingleIconDetails(sb, icon, variant, size);

        // Code
        AppendSingleIconUsage(sb, icon.Name, variant, size);

        return sb.ToString();
    }

    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    private void AppendSingleIconPreview(StringBuilder sb, IconModel icon, string variant, int size)
    {
        sb.AppendLine("## Preview");
        sb.AppendLine();

        var svgContent = _svgProvider.GetSvgContent(icon.Name, variant, size);
        if (string.IsNullOrEmpty(svgContent))
        {
            sb.AppendLine("*(Preview not available for this variant/size combination)*");
            sb.AppendLine();
            return;
        }

        // Multiple display sizes
        int[] displaySizes = [64, 32, 20, 16];
        foreach (var displaySize in displaySizes)
        {
            var svg = $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {size} {size}\" width=\"{displaySize}\" height=\"{displaySize}\" fill=\"currentColor\">{svgContent}</svg>";
            var encoded = HttpUtility.UrlEncode(svg).Replace("+", "%20", StringComparison.Ordinal);

            if (displaySize == 64)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"![{icon.Name} {displaySize}px](data:image/svg+xml,{encoded})");
                sb.AppendLine();
            }
            else
            {
                sb.Append(CultureInfo.InvariantCulture, $"![{icon.Name} {displaySize}px](data:image/svg+xml,{encoded}) ");
            }
        }

        sb.AppendLine();
        sb.AppendLine();
    }

    private static void AppendSingleIconDetails(StringBuilder sb, IconModel icon, string variant, int size)
    {
        sb.AppendLine("## Details");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"- **Variant:** {variant}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"- **Size:** {size}px");
        sb.AppendLine(CultureInfo.InvariantCulture, $"- **Available variants:** {string.Join(", ", icon.VariantNames)}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"- **Available sizes:** {string.Join(", ", icon.AllSizes.Select(s => $"{s}px"))}");
        sb.AppendLine();
    }

    private static void AppendSingleIconUsage(StringBuilder sb, string iconName, string variant, int size)
    {
        sb.AppendLine("## Usage");
        sb.AppendLine();
        sb.AppendLine("**Blazor component:**");
        sb.AppendLine("```razor");
        sb.AppendLine(CultureInfo.InvariantCulture, $"<FluentIcon Value=\"@(new Icons.{variant}.Size{size}.{iconName}())\" />");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("**C# code:**");
        sb.AppendLine("```csharp");
        sb.AppendLine(CultureInfo.InvariantCulture, $"var icon = new Icons.{variant}.Size{size}.{iconName}();");
        sb.AppendLine("```");
    }

    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    private string FormatIconNotFound(string iconName)
    {
        var suggestions = _iconService.SearchIcons(iconName);
        if (suggestions.Count > 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine(CultureInfo.InvariantCulture, $"Icon '{iconName}' not found. Did you mean one of these?");
            sb.AppendLine();
            foreach (var s in suggestions.Take(10))
            {
                var (defaultVariant, defaultSize) = _iconService.GetRecommendedDefault(s);
                var preview = GetSvgPreview(s.Name, defaultVariant, defaultSize);
                sb.AppendLine(CultureInfo.InvariantCulture, $"- {preview} **{s.Name}** ({string.Join(", ", s.VariantNames)})");
            }

            return sb.ToString();
        }

        return $"Icon '{iconName}' not found in the catalog. Use SearchIcons to discover available icons.";
    }

    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    private string FormatIconDetails(IconModel icon)
    {
        var result = new StringBuilder();
        var (defaultVariant, defaultSize) = _iconService.GetRecommendedDefault(icon);

        result.AppendLine(CultureInfo.InvariantCulture, $"# {icon.Name}");
        result.AppendLine();

        // Add visual preview
        AppendIconPreviewSection(result, icon);

        result.AppendLine(CultureInfo.InvariantCulture, $"**Recommended default:** `Icons.{defaultVariant}.Size{defaultSize}.{icon.Name}`");
        result.AppendLine();

        AppendVariantSizeMatrix(result, icon);
        AppendQuickStart(result, icon.Name, defaultVariant, defaultSize);

        return result.ToString();
    }

    /// <summary>
    /// Appends a visual preview section showing the icon in different variants.
    /// </summary>
    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    private void AppendIconPreviewSection(StringBuilder sb, IconModel icon)
    {
        sb.AppendLine("## Visual Preview");
        sb.AppendLine();

        // Show preview for each variant
        var previewsShown = 0;
        foreach (var kvp in icon.Variants.OrderBy(v => v.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (kvp.Key.Equals("Color", StringComparison.OrdinalIgnoreCase))
            {
                continue; // Skip Color variant as it's not available via standard classes
            }

            // Get preferred size for preview (20 or 24 or first available)
            var previewSize = kvp.Value.Contains(20) ? 20 : (kvp.Value.Contains(24) ? 24 : kvp.Value.FirstOrDefault());
            if (previewSize == 0)
            {
                continue;
            }

            var preview = GetSvgPreviewLarge(icon.Name, kvp.Key, previewSize);
            if (preview != null)
            {
                sb.Append(CultureInfo.InvariantCulture, $"{preview} ");
                previewsShown++;
            }
        }

        if (previewsShown > 0)
        {
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("*(Variants shown: " + string.Join(", ", icon.Variants.Keys.Where(k => !k.Equals("Color", StringComparison.OrdinalIgnoreCase))) + ")*");
            sb.AppendLine();
        }
    }

    /// <summary>
    /// Gets a larger SVG preview for detailed view.
    /// </summary>
    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    private string? GetSvgPreviewLarge(string iconName, string variant, int size)
    {
        var svgContent = _svgProvider.GetSvgContent(iconName, variant, size);
        if (string.IsNullOrEmpty(svgContent))
        {
            return null;
        }

        // Create a larger SVG for detailed preview
        var fullSvg = $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {size} {size}\" width=\"32\" height=\"32\" fill=\"currentColor\">{svgContent}</svg>";
        var encodedSvg = HttpUtility.UrlEncode(fullSvg).Replace("+", "%20", StringComparison.Ordinal);
        return $"![{iconName} {variant}](data:image/svg+xml,{encodedSvg})";
    }

    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    private void AppendVariantSizeMatrix(StringBuilder sb, IconModel icon)
    {
        sb.AppendLine("## Available Variants and Sizes");
        sb.AppendLine();

        foreach (var kvp in icon.Variants.OrderBy(v => v.Key, StringComparer.OrdinalIgnoreCase))
        {
            var sizes = string.Join(", ", kvp.Value);
            var isColor = kvp.Key.Equals("Color", StringComparison.OrdinalIgnoreCase);
            sb.AppendLine(CultureInfo.InvariantCulture, $"### {kvp.Key}{(isColor ? " ⚠️ (multi-color, not available via standard class hierarchy)" : "")}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Sizes: {sizes}");
            sb.AppendLine();

            if (!isColor)
            {
                // Show a row of size previews
                var previews = new List<string>();
                foreach (var size in kvp.Value.Take(6)) // Limit to 6 sizes for readability
                {
                    var preview = GetSvgPreview(icon.Name, kvp.Key, size);
                    previews.Add($"{preview} {size}px");
                }

                if (previews.Count > 0)
                {
                    sb.AppendLine(string.Join(" | ", previews));
                    sb.AppendLine();
                }

                foreach (var size in kvp.Value)
                {
                    sb.AppendLine(CultureInfo.InvariantCulture, $"- `new Icons.{kvp.Key}.Size{size}.{icon.Name}()`");
                }

                sb.AppendLine();
            }
        }
    }

    private static void AppendQuickStart(StringBuilder sb, string iconName, string variant, int size)
    {
        sb.AppendLine("## Quick Start");
        sb.AppendLine("```razor");
        sb.AppendLine(CultureInfo.InvariantCulture, $"<FluentIcon Value=\"@(new Icons.{variant}.Size{size}.{iconName}())\" />");
        sb.AppendLine("```");
        sb.AppendLine();
    }

    /// <summary>
    /// Generates ready-to-use Blazor code snippets for a specific icon.
    /// Shows multiple usage patterns: basic, with color, in a button, with click handler, etc.
    /// </summary>
    [McpServerTool]
    [Description("Generate ready-to-use Blazor code snippets for a specific Fluent UI icon. Shows multiple usage patterns including basic rendering, color options, button slots, click handlers, and accessibility attributes.")]
    public string GetIconUsage(
        [Description("The exact icon name (e.g., 'Bookmark', 'Alert', 'Calendar').")]
        string iconName,
        [Description("The variant to use: 'Filled' (solid), 'Regular' (outlined), or 'Light' (thin). Default: Regular.")]
        string variant = "Regular",
        [Description("The icon size: 10, 12, 16, 20, 24, 28, 32, or 48. Default: 20.")]
        int size = 20)
    {
        return _iconService.GenerateUsageExamples(iconName, variant, size);
    }

    /// <summary>
    /// Lists all available Fluent UI icon names, optionally filtered by a starting letter or prefix.
    /// Use this to browse the full icon catalog and pick the right icon name for other tools.
    /// </summary>
    [McpServerTool]
    [Description("Lists all available Fluent UI icon names. Use an optional prefix to filter (e.g., 'A', 'Arrow', 'Cal'). Call this first to discover exact icon names, then use SearchIcons, GetIconDetails, or GetIconUsage.")]
    public string ListAllIconNames(
        [Description("Optional: A prefix to filter icon names (e.g., 'A' for all icons starting with A, 'Arrow' for arrow icons). Leave empty for the full list grouped by first letter.")]
        string? prefix = null)
    {
        var allIcons = _iconService.GetAllIcons();
        return FormatIconNameList(allIcons, prefix);
    }

    private static string FormatIconNameList(IReadOnlyList<IconModel> allIcons, string? prefix)
    {
        var icons = allIcons
            .Select(i => i.Name)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(prefix))
        {
            return FormatFilteredList(icons, prefix, allIcons.Count);
        }

        return FormatGroupedList(icons, allIcons.Count);
    }

    private static string FormatFilteredList(IEnumerable<string> icons, string prefix, int totalCount)
    {
        var filtered = icons
            .Where(n => n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"# Icons starting with '{prefix}' ({filtered.Count} of {totalCount} total)");
        sb.AppendLine();

        if (filtered.Count == 0)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"No icons found starting with '{prefix}'. Try a different prefix or omit it to see all icons grouped by letter.");
            return sb.ToString();
        }

        foreach (var name in filtered)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"- {name}");
        }

        sb.AppendLine();
        sb.AppendLine("Use `GetIconDetails(iconName)` to see variants and sizes for a specific icon.");
        return sb.ToString();
    }

    private static string FormatGroupedList(IEnumerable<string> icons, int totalCount)
    {
        var grouped = icons.GroupBy(n => char.ToUpperInvariant(n[0]));

        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"# All Fluent UI Icons ({totalCount} icons)");
        sb.AppendLine();
        sb.AppendLine("Icons grouped by first letter. Use the `prefix` parameter to filter (e.g., prefix='Arrow').");
        sb.AppendLine();

        foreach (var group in grouped.OrderBy(g => g.Key))
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"## {group.Key} ({group.Count()} icons)");
            sb.AppendLine(string.Join(", ", group));
            sb.AppendLine();
        }

        sb.AppendLine("Use `GetIconDetails(iconName)` to see variants and sizes for a specific icon.");
        return sb.ToString();
    }
}
