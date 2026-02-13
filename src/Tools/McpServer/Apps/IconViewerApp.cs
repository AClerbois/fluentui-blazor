// ------------------------------------------------------------------------
// This file is licensed to you under the MIT License.
// ------------------------------------------------------------------------

using System.ComponentModel;
using Microsoft.FluentUI.AspNetCore.McpServer.Models;
using ModelContextProtocol.Server;

namespace Microsoft.FluentUI.AspNetCore.McpServer.Apps;

/// <summary>
/// MCP App for viewing a single Fluent UI icon with visual preview.
/// </summary>
[McpServerToolType]
public class IconViewerApp
{
    /// <summary>
    /// Shows a single Fluent UI icon with a large visual preview in an interactive UI.
    /// </summary>
    [McpServerTool]
    [Description("Display a single Fluent UI icon with a large visual preview. Shows the icon in all variants and sizes with code snippets ready to copy.")]
    [McpMeta("ui", JsonValue = """{ "resourceUri": "ui://fluent-icons/viewer.html" }""")]
    public IconViewerResult ShowIconVisual(
        [Description("The exact icon name (e.g., 'Bookmark', 'Alert', 'Calendar', 'DocumentEdit').")]
        string iconName,
        [Description("Optional: The preferred variant — 'Filled' (solid), 'Regular' (outlined), 'Light' (thin). Defaults to Regular.")]
        string? variant = null,
        [Description("Optional: The preferred size — 10, 12, 16, 20, 24, 28, 32, or 48. Defaults to 20.")]
        int? size = null)
    {
        return new IconViewerResult
        {
            IconName = iconName,
            Variant = variant ?? "Regular",
            Size = size ?? 20,
            Message = $"Showing icon: {iconName}"
        };
    }
}
