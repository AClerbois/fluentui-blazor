// ------------------------------------------------------------------------
// This file is licensed to you under the MIT License.
// ------------------------------------------------------------------------

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using ModelContextProtocol.Server;
using Microsoft.FluentUI.AspNetCore.McpServer.Helpers;
using Microsoft.FluentUI.AspNetCore.McpServer.Services;

namespace Microsoft.FluentUI.AspNetCore.McpServer.Resources;

/// <summary>
/// MCP resource serving the single Icon Viewer UI.
/// </summary>
[McpServerResourceType]
public class IconViewerResource
{
    private readonly IconService _iconService;
    private readonly IconSvgProvider _svgProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="IconViewerResource"/> class.
    /// </summary>
    public IconViewerResource(IconService iconService, IconSvgProvider svgProvider)
    {
        _iconService = iconService;
        _svgProvider = svgProvider;
    }

    /// <summary>
    /// Provides the interactive HTML UI for viewing a single Fluent UI icon.
    /// </summary>
    /// <param name="iconName">The icon name to display.</param>
    /// <param name="variant">Optional preferred variant (Filled, Regular, Light).</param>
    /// <param name="size">Optional preferred size.</param>
    [McpServerResource(
        UriTemplate = "ui://fluent-icons/viewer.html{?iconName,variant,size}",
        MimeType = "text/html",
        Title = "Fluent UI Icon Viewer")]
    [Description("Interactive UI for viewing a single Fluent UI icon with all variants and sizes")]
    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    public Task<string> GetIconViewerUIAsync(
        string? iconName = null,
        string? variant = null,
        string? size = null)
    {
        return Task.FromResult(IconViewerHtml.GetHtml(_iconService, _svgProvider, iconName, variant, size));
    }
}
