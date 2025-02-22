// ------------------------------------------------------------------------
// MIT License - Copyright (c) Microsoft Corporation. All rights reserved.
// ------------------------------------------------------------------------

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Microsoft.FluentUI.AspNetCore.Components;

/// <summary>
/// FluentTabs component. 
/// </summary>
public partial class FluentTabs : FluentComponentBase
{
    private readonly List<FluentTab> _tabs = [];

    /// <summary />
    protected string? ClassValue => DefaultClassBuilder
        .Build();

    /// <summary />
    protected string? StyleValue => DefaultStyleBuilder
        .AddStyle("padding", "6px", () => Size == TabsSize.Small)
        .AddStyle("padding", "12px 10px", () => Size == TabsSize.Medium)
        .AddStyle("padding", "16px 10px", () => Size == TabsSize.Large)
        .AddStyle("width", Width, () => !string.IsNullOrEmpty(Width))
        .AddStyle("height", Height, () => !string.IsNullOrEmpty(Height))
        .Build();

    /// <inheritdoc cref="IFluentComponentElementBase.Element" />
    [Parameter]
    public ElementReference Element { get; set; }

    /// <summary>
    /// Gets or sets the appearance of the tabs.
    /// </summary>
    [Parameter]
    public TabsAppearance? Appearance { get; set; }

    /// <summary>
    /// Get or sets tab size may change between unselected and selected states.
    /// The default scenario is a selected tab has bold text.
    /// </summary>
    [Parameter]
    public bool? ReserveSelectedTabSpace { get; set; }

    /// <summary>
    /// Get or sets the tabs disabled state.
    /// </summary>
    [Parameter]
    public bool? Disabled { get; set; }

    /// <summary>
    /// Gets or sets the component size.
    /// </summary>
    [Parameter]
    public TabsSize? Size { get; set; }

    /// <summary>
    /// Gets or sets the width of the tabs component.
    /// Needs to be a valid CSS value (e.g. 100px, 50%).
    /// </summary>
    [Parameter]
    public string? Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the tabs component.
    /// Needs to be a valid CSS value (e.g. 100px, 50%).
    /// </summary>
    [Parameter]
    public string? Height { get; set; }

    /// <summary>
    /// Gets or sets the tabs vertical state.
    /// </summary>
    [Parameter]
    public bool? Vertical { get; set; }

    /// <summary>
    /// Gets or sets the content for the tabs.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    private string _currentActiveId = default!;

    private string CurrentActiveId
    {
        get => _currentActiveId;
        set
        {
            if (!string.Equals(_currentActiveId, value, StringComparison.OrdinalIgnoreCase))
            {
                _currentActiveId = value;
                if (ActiveTabIdChanged.HasDelegate)
                {
                    _ = InvokeActiveTabIdChangedAsync(value);
                }
            }
        }
    }

    private async Task InvokeActiveTabIdChangedAsync(string value)
    {
        await ActiveTabIdChanged.InvokeAsync(value).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets or sets the active tab id.
    /// </summary>
    [Parameter]
    public string ActiveId { get; set; } = default!;

    /// <summary>
    /// Gets or sets a callback when the bound value is changed.
    /// </summary>
    [Parameter]
    public EventCallback<string> ActiveTabIdChanged { get; set; }

    /// <summary>
    /// Gets or sets a callback when a tab is changed.
    /// </summary>
    [Parameter]
    public EventCallback<FluentTab> OnTabChange { get; set; }

    /// <summary>
    /// Gets the active selected tab.
    /// </summary>
    public FluentTab ActiveTab => _tabs.Find(t => string.Equals(t.Id, ActiveId, StringComparison.Ordinal)) ?? _tabs[0];

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("Microsoft.FluentUI.Blazor.Utilities.Attributes.observeAttributeChange", Element, "activeid");
        }
    }

    private string? GetOrientation()
        => Vertical.HasValue
            ? Vertical.Value
                ? "vertical"
                : "horizontal"
            : null;

    [JSInvokable("OnHandleOnTabChangedAsync")]
    private void HandleOnTabChanged(string newValue, string oldValue)
    {
        var tabId = newValue;
        var tab = _tabs.Find(i => string.Equals(i.Id, tabId, StringComparison.Ordinal));

        if (tab is not null && _tabs.Contains(tab))
        {

            if (tabId != null)
            {
                ActiveId = tabId;
                if (ActiveTabIdChanged.HasDelegate)
                {

                }
            }
        }
    }

    internal int RegisterTab(FluentTab tab)
    {
        //if (ActiveId is null && tab.Id is not null)
        //{
        //    ActiveId = tab.Id;
        //}

        _tabs.Add(tab);
        return _tabs.Count - 1;
    }
}
