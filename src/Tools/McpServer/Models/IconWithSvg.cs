// ------------------------------------------------------------------------
// This file is licensed to you under the MIT License.
// ------------------------------------------------------------------------

using System.Text.Json.Serialization;

namespace Microsoft.FluentUI.AspNetCore.McpServer.Models;

/// <summary>
/// Represents a Fluent UI icon with its SVG content for a specific variant and size.
/// </summary>
/// <param name="Name">The icon name (e.g., "Bookmark", "Calendar").</param>
/// <param name="Variant">The variant (e.g., "Filled", "Regular").</param>
/// <param name="Size">The icon size (e.g., 20, 24).</param>
/// <param name="Svg">The SVG path content.</param>
/// <param name="AllVariants">Dictionary of all available variants and their sizes.</param>
public sealed record IconWithSvg(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("variant")] string Variant,
    [property: JsonPropertyName("size")] int Size,
    [property: JsonPropertyName("svg")] string Svg,
    [property: JsonPropertyName("allVariants")] IDictionary<string, IList<int>> AllVariants)
{
    /// <summary>
    /// Generates the Blazor code snippet for this icon.
    /// </summary>
    [JsonPropertyName("code")]
    public string Code => $"<FluentIcon Value=\"@(new Icons.{Variant}.Size{Size}.{Name}())\" />";

    /// <summary>
    /// Gets the full SVG markup for rendering.
    /// </summary>
    [JsonPropertyName("svgMarkup")]
    public string SvgMarkup => $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {Size} {Size}\" fill=\"currentColor\">{Svg}</svg>";
}
