// ------------------------------------------------------------------------
// This file is licensed to you under the MIT License.
// ------------------------------------------------------------------------

namespace Microsoft.FluentUI.AspNetCore.McpServer.Models;

/// <summary>
/// Result returned when showing a single icon.
/// </summary>
public class IconViewerResult
{
    /// <summary>
    /// Gets or sets the icon name to display.
    /// </summary>
    public string IconName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the preferred variant.
    /// </summary>
    public string Variant { get; set; } = "Regular";

    /// <summary>
    /// Gets or sets the preferred size.
    /// </summary>
    public int Size { get; set; } = 20;

    /// <summary>
    /// Gets or sets a status message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
