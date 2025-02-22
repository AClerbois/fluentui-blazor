// ------------------------------------------------------------------------
// MIT License - Copyright (c) Microsoft Corporation. All rights reserved.
// ------------------------------------------------------------------------

using System.ComponentModel;

namespace Microsoft.FluentUI.AspNetCore.Components;

/// <summary>
/// Represents the apprearance of the <see cref="FluentTabs"/>.
/// </summary>
public enum TabsAppearance
{
    /// <summary>
    /// No background and border styling - Default appearance.
    /// </summary>
    [Description("transparent")]
    Transparent,

    /// <summary>
    /// Minimizes emphasis to blend into the background until hovered or focused.
    /// </summary>
    [Description("subtle")]
    Subtle,
}
