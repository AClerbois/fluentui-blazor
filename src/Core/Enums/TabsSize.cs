// ------------------------------------------------------------------------
// MIT License - Copyright (c) Microsoft Corporation. All rights reserved.
// ------------------------------------------------------------------------

using System.ComponentModel;

namespace Microsoft.FluentUI.AspNetCore.Components;

/// <summary>
/// Represents the size of the <see cref="FluentTabs"/>.
/// </summary>
public enum TabsSize
{
    /// <summary>
    /// Medium tab size - Default size.
    /// </summary>
    [Description("medium")]
    Medium,

    /// <summary>
    /// Small tab size.
    /// </summary>
    [Description("small")]
    Small,

    /// <summary>
    /// Large tab size.
    /// </summary>  
    [Description("large")]
    Large,
}
