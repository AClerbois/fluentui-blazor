// ------------------------------------------------------------------------
// This file is licensed to you under the MIT License.
// ------------------------------------------------------------------------

using System.Text.Json.Serialization;

namespace Microsoft.FluentUI.AspNetCore.McpServer.Models;

/// <summary>
/// Represents a compact icon entry for the explorer UI.
/// </summary>
/// <param name="Name">The icon name.</param>
/// <param name="Variants">List of available variants.</param>
/// <param name="Sizes">List of available sizes.</param>
/// <param name="DefaultSvg">SVG content for the default variant/size.</param>
public sealed record IconExplorerEntry(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("variants")] IList<string> Variants,
    [property: JsonPropertyName("sizes")] IList<int> Sizes,
    [property: JsonPropertyName("defaultSvg")] string? DefaultSvg = null)
{
    /// <summary>
    /// Dictionary of variant to sizes mapping.
    /// </summary>
    [JsonPropertyName("variantSizes")]
    public IDictionary<string, IList<int>>? VariantSizes { get; init; }
}
