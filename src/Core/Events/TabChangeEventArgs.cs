// ------------------------------------------------------------------------
// MIT License - Copyright (c) Microsoft Corporation. All rights reserved.
// ------------------------------------------------------------------------

namespace Microsoft.FluentUI.AspNetCore.Components;

/// <summary>
/// Event arguments for the Tab change event <see cref="FluentTabs"/>.
/// </summary>
public class TabChangeEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the ID of the active tab.
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// Gets or sets the ID of the tab that was previously active.
    /// </summary>
    public string? OldValue { get; set; }
}
