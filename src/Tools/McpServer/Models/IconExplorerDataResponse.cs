// ------------------------------------------------------------------------
// This file is licensed to you under the MIT License.
// ------------------------------------------------------------------------

using System.Text.Json.Serialization;

namespace Microsoft.FluentUI.AspNetCore.McpServer.Models;

/// <summary>
/// Response payload for the icon explorer data endpoint.
/// </summary>
public sealed record IconExplorerDataResponse
{
    /// <summary>
    /// Total number of icons available.
    /// </summary>
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; init; }

    /// <summary>
    /// List of icon entries.
    /// </summary>
    [JsonPropertyName("icons")]
    public IList<IconExplorerEntry> Icons { get; init; } = [];

    /// <summary>
    /// Available variants.
    /// </summary>
    [JsonPropertyName("availableVariants")]
    public IList<string> AvailableVariants { get; init; } = ["Filled", "Regular", "Light", "Color"];

    /// <summary>
    /// Available sizes.
    /// </summary>
    [JsonPropertyName("availableSizes")]
    public IList<int> AvailableSizes { get; init; } = [10, 12, 16, 20, 24, 28, 32, 48];
}
