// ------------------------------------------------------------------------
// This file is licensed to you under the MIT License.
// ------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.FluentUI.AspNetCore.McpServer.Models;
using Microsoft.FluentUI.AspNetCore.McpServer.Services;

namespace Microsoft.FluentUI.AspNetCore.McpServer.Helpers;

/// <summary>
/// HTML provider for the single icon viewer UI.
/// </summary>
public static class IconViewerHtml
{
    /// <summary>
    /// Gets the HTML for viewing a single icon.
    /// </summary>
    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    public static string GetHtml(
        IconService iconService,
        IconSvgProvider svgProvider,
        string? iconName = null,
        string? variant = null,
        string? size = null)
    {
        // Build ALL icons data so the UI can look up any icon when receiving tool result
        var allIconsData = BuildAllIconsData(iconService, svgProvider);
        var allIconsJson = JsonSerializer.Serialize(allIconsData, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var jsIconName = EscapeJsString(iconName);
        var jsVariant = EscapeJsString(variant);
        var jsSize = EscapeJsString(size);

        return BuildHtmlPage(allIconsJson, jsIconName, jsVariant, jsSize);
    }

    private static string BuildHtmlPage(string allIconsJson, string jsIconName, string jsVariant, string jsSize)
    {
        return $$"""
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>Fluent UI Icon Viewer</title>
  <style>{{GetCssStyles()}}</style>
</head>
<body>
{{GetHtmlBody()}}
<script type="module">{{GetJavaScript(allIconsJson, jsIconName, jsVariant, jsSize)}}</script>
</body>
</html>
""";
    }

    private static string GetCssStyles()
    {
        return """

    * { box-sizing: border-box; margin: 0; padding: 0; }
    html, body { height: 100%; overflow: hidden; }
    body {
      font-family: var(--font-sans, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif);
      background: var(--color-background-primary, #1e1e1e);
      color: var(--color-text-primary, #cccccc);
      padding: 16px;
      display: flex;
      flex-direction: column;
    }
    .container { max-width: 600px; margin: 0 auto; height: 100%; display: flex; flex-direction: column; gap: 16px; }
    .header { text-align: center; flex-shrink: 0; }
    h1 { font-size: 24px; font-weight: 600; color: var(--color-text-primary, #ffffff); margin-bottom: 4px; }
    .subtitle { font-size: 13px; color: var(--color-text-secondary, #888888); }
    .loading { text-align: center; padding: 48px; color: var(--color-text-secondary, #888888); }
    .loading-spinner { font-size: 32px; animation: spin 1s linear infinite; display: inline-block; }
    @keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
    .preview-container { background: var(--color-background-secondary, #2d2d2d); border: 1px solid var(--color-border-primary, #3c3c3c); border-radius: 8px; padding: 24px; text-align: center; flex-shrink: 0; }
    .preview-large { display: flex; align-items: center; justify-content: center; margin-bottom: 16px; }
    .preview-large svg { width: 96px; height: 96px; fill: currentColor; }
    .preview-sizes { display: flex; align-items: center; justify-content: center; gap: 16px; padding: 12px; background: var(--color-background-primary, #1e1e1e); border-radius: 4px; flex-wrap: wrap; }
    .preview-size { display: flex; flex-direction: column; align-items: center; gap: 4px; cursor: pointer; padding: 8px; border-radius: 4px; transition: background 0.2s; }
    .preview-size:hover { background: var(--color-background-hover, #3c3c3c); }
    .preview-size.active { background: var(--color-border-focus, #007acc); }
    .preview-size-label { font-size: 10px; color: var(--color-text-secondary, #888888); }
    .variant-selector { display: flex; gap: 8px; justify-content: center; flex-wrap: wrap; flex-shrink: 0; }
    .chip { padding: 6px 14px; border: 1px solid var(--color-border-primary, #3c3c3c); border-radius: 16px; font-size: 12px; cursor: pointer; transition: all 0.2s; background: var(--color-background-secondary, #2d2d2d); color: var(--color-text-primary, #cccccc); }
    .chip:hover { border-color: var(--color-border-focus, #007acc); }
    .chip.active { background: var(--color-border-focus, #007acc); border-color: var(--color-border-focus, #007acc); color: #ffffff; }
    .info-section { flex-shrink: 0; }
    .info-grid { display: grid; gap: 6px; }
    .info-row { display: flex; justify-content: space-between; padding: 8px 12px; background: var(--color-background-secondary, #2d2d2d); border-radius: 4px; font-size: 13px; }
    .info-label { color: var(--color-text-secondary, #888888); }
    .info-value { color: var(--color-text-primary, #ffffff); font-weight: 500; }
    .code-section { flex: 1; min-height: 0; overflow: auto; }
    .code-block { background: var(--color-background-secondary, #2d2d2d); border: 1px solid var(--color-border-primary, #3c3c3c); border-radius: 4px; padding: 12px; font-family: var(--font-mono, 'Consolas', 'Monaco', monospace); font-size: 12px; color: var(--color-text-primary, #d4d4d4); position: relative; margin-bottom: 8px; }
    .code-label { font-size: 11px; color: var(--color-text-secondary, #888888); margin-bottom: 4px; }
    .copy-btn { position: absolute; top: 8px; right: 8px; background: var(--color-background-primary, #1e1e1e); border: 1px solid var(--color-border-primary, #3c3c3c); border-radius: 4px; padding: 4px 8px; color: var(--color-text-secondary, #888888); cursor: pointer; font-size: 11px; transition: all 0.2s; }
    .copy-btn:hover { background: var(--color-background-hover, #3c3c3c); color: var(--color-text-primary, #ffffff); }
    .error-state { text-align: center; padding: 48px; color: var(--color-text-secondary, #888888); }
    .error-icon { font-size: 48px; margin-bottom: 16px; }
  """;
    }

    private static string GetHtmlBody()
    {
        return """
  <div class="container">
    <div class="header">
      <h1 id="icon-name">Loading...</h1>
      <p class="subtitle" id="icon-subtitle"></p>
    </div>
    <div id="loading" class="loading">
      <div class="loading-spinner">⏳</div>
      <p>Waiting for icon selection...</p>
    </div>
    <div id="content" style="display: none;">
      <div class="preview-container">
        <div class="preview-large" id="preview-large"></div>
        <div class="preview-sizes" id="preview-sizes"></div>
      </div>
      <div class="variant-selector" id="variant-selector"></div>
      <div class="info-section">
        <div class="info-grid" id="info-grid"></div>
      </div>
      <div class="code-section">
        <div class="code-label">Blazor Component</div>
        <div class="code-block">
          <button class="copy-btn" onclick="copyCode('blazor-code')">Copy</button>
          <code id="blazor-code"></code>
        </div>
        <div class="code-label">C# Code</div>
        <div class="code-block">
          <button class="copy-btn" onclick="copyCode('csharp-code')">Copy</button>
          <code id="csharp-code"></code>
        </div>
      </div>
    </div>
    <div id="error-state" class="error-state" style="display: none;">
      <div class="error-icon">❓</div>
      <div id="error-message">Icon not found</div>
    </div>
  </div>
""";
    }

#pragma warning disable MA0051 // Method is too long
    private static string GetJavaScript(string allIconsJson, string jsIconName, string jsVariant, string jsSize)
    {
        return $$"""

    // All icons data embedded from server
    const ALL_ICONS = {{allIconsJson}};
    
    // Current state
    let currentIconName = '{{jsIconName}}' || null;
    let currentVariant = '{{jsVariant}}' || 'Regular';
    let currentSize = parseInt('{{jsSize}}') || 20;
    let currentIconData = null;

    // Handle tool result from MCP host
    function handleToolResult(result) {
      console.log('handleToolResult:', result);
      if (!result) return;
      
      // Extract iconName from various result formats
      let iconName = result.iconName || result.IconName || result.name || result.Name;
      if (result.content && Array.isArray(result.content)) {
        const textContent = result.content.find(c => c.type === 'text');
        if (textContent && textContent.text) {
          try {
            const parsed = JSON.parse(textContent.text);
            iconName = parsed.iconName || parsed.IconName || parsed.name || iconName;
            if (parsed.variant || parsed.Variant) currentVariant = parsed.variant || parsed.Variant;
            if (parsed.size || parsed.Size) currentSize = parseInt(parsed.size || parsed.Size) || 20;
          } catch (e) { /* not JSON */ }
        }
      }
      
      if (result.variant || result.Variant) currentVariant = result.variant || result.Variant;
      if (result.size || result.Size) currentSize = parseInt(result.size || result.Size) || 20;
      
      if (iconName) {
        currentIconName = iconName;
        loadIcon(iconName);
      }
    }

    // Listen for messages from the MCP host
    window.addEventListener('message', (event) => {
      console.log('IconViewer received message:', JSON.stringify(event.data, null, 2));
      const data = event.data;
      if (!data) return;

      // Handle MCP Apps JSON-RPC protocol
      if (data.jsonrpc === '2.0') {
        if (data.method === 'ui/toolResult' || data.method === 'notifications/toolResult') {
          handleToolResult(data.params?.result || data.params);
          return;
        }
        if (data.method === 'ui/initialize' || data.method === 'initialize') {
          const params = data.params || {};
          if (params.toolInput) {
            const args = params.toolInput.arguments || params.toolInput;
            if (args.iconName) { currentIconName = args.iconName; loadIcon(args.iconName); }
          }
          if (params.toolResult) handleToolResult(params.toolResult);
          return;
        }
      }

      // Legacy formats
      if (data.type === 'tool-input' || data.type === 'init' || data.toolInput) {
        const args = data.arguments || data.args || data.params || data.toolInput?.arguments || data.toolInput || data;
        if (args.iconName) { currentIconName = args.iconName; loadIcon(args.iconName); }
      } else if (data.toolResult) {
        handleToolResult(data.toolResult);
      } else if (data.iconName) {
        currentIconName = data.iconName;
        if (data.variant) currentVariant = data.variant;
        if (data.size) currentSize = parseInt(data.size) || 20;
        loadIcon(data.iconName);
      }
    });

    function loadIcon(iconName) {
      console.log('loadIcon:', iconName);
      const iconData = ALL_ICONS[iconName];
      if (!iconData) {
        showError(`Icon "${iconName}" not found in catalog.`);
        return;
      }
      
      currentIconData = iconData;
      
      // Validate variant
      if (!iconData.variants.includes(currentVariant)) {
        currentVariant = iconData.variants[0] || 'Regular';
      }
      
      // Validate size
      const variantSizes = iconData.variantSizes?.[currentVariant] || iconData.sizes;
      if (!variantSizes.includes(currentSize)) {
        currentSize = variantSizes.includes(20) ? 20 : variantSizes[0];
      }
      
      // Show the content
      document.getElementById('loading').style.display = 'none';
      document.getElementById('content').style.display = 'block';
      document.getElementById('error-state').style.display = 'none';
      
      document.getElementById('icon-name').textContent = iconData.name;
      document.getElementById('icon-subtitle').textContent = 
        `${iconData.variants.length} variant${iconData.variants.length > 1 ? 's' : ''} • ${iconData.sizes.length} size${iconData.sizes.length > 1 ? 's' : ''}`;
      
      renderAll();
    }

    function showError(message) {
      document.getElementById('loading').style.display = 'none';
      document.getElementById('content').style.display = 'none';
      document.getElementById('error-state').style.display = 'block';
      document.getElementById('error-message').textContent = message;
      document.getElementById('icon-name').textContent = 'Error';
      document.getElementById('icon-subtitle').textContent = '';
    }

    function renderAll() {
      renderVariantSelector();
      renderPreview();
      renderInfo();
      renderCode();
    }

    function renderVariantSelector() {
      if (!currentIconData) return;
      const container = document.getElementById('variant-selector');
      container.innerHTML = currentIconData.variants.map(v => 
        `<div class="chip ${v === currentVariant ? 'active' : ''}" data-variant="${v}">${v}</div>`
      ).join('');
      container.querySelectorAll('.chip').forEach(chip => {
        chip.addEventListener('click', () => {
          currentVariant = chip.dataset.variant;
          const variantSizes = currentIconData.variantSizes?.[currentVariant] || currentIconData.sizes;
          if (!variantSizes.includes(currentSize)) {
            currentSize = variantSizes.includes(20) ? 20 : variantSizes[0];
          }
          renderAll();
        });
      });
    }

    function renderPreview() {
      if (!currentIconData) return;
      const largeSvg = getSvg(currentVariant, currentSize);
      document.getElementById('preview-large').innerHTML = largeSvg 
        ? `<svg viewBox="0 0 ${currentSize} ${currentSize}" fill="currentColor">${largeSvg}</svg>`
        : '<span style="font-size:48px">❓</span>';
      
      const variantSizes = currentIconData.variantSizes?.[currentVariant] || currentIconData.sizes;
      document.getElementById('preview-sizes').innerHTML = variantSizes.map(s => {
        const svg = getSvg(currentVariant, s);
        const displaySize = Math.min(s, 32);
        const isActive = s === currentSize;
        return `<div class="preview-size ${isActive ? 'active' : ''}" data-size="${s}">
          ${svg ? `<svg viewBox="0 0 ${s} ${s}" width="${displaySize}" height="${displaySize}" fill="currentColor">${svg}</svg>` : '❓'}
          <span class="preview-size-label">${s}px</span>
        </div>`;
      }).join('');
      
      document.querySelectorAll('.preview-size').forEach(el => {
        el.addEventListener('click', () => {
          currentSize = parseInt(el.dataset.size);
          renderAll();
        });
      });
    }

    function getSvg(variant, size) {
      if (!currentIconData?.svgByVariant?.[variant]) return null;
      return currentIconData.svgByVariant[variant][size] || 
             currentIconData.svgByVariant[variant][Object.keys(currentIconData.svgByVariant[variant])[0]];
    }

    function renderInfo() {
      if (!currentIconData) return;
      const variantSizes = currentIconData.variantSizes?.[currentVariant] || currentIconData.sizes;
      document.getElementById('info-grid').innerHTML = `
        <div class="info-row"><span class="info-label">Variant</span><span class="info-value">${currentVariant}</span></div>
        <div class="info-row"><span class="info-label">Size</span><span class="info-value">${currentSize}px</span></div>
        <div class="info-row"><span class="info-label">Available Sizes</span><span class="info-value">${variantSizes.join(', ')}</span></div>
      `;
    }

    function renderCode() {
      if (!currentIconData) return;
      document.getElementById('blazor-code').textContent = 
        `<FluentIcon Value="@(new Icons.${currentVariant}.Size${currentSize}.${currentIconData.name}())" />`;
      document.getElementById('csharp-code').textContent = 
        `new Icons.${currentVariant}.Size${currentSize}.${currentIconData.name}()`;
    }

    window.copyCode = async (elementId) => {
      const code = document.getElementById(elementId).textContent;
      try {
        await navigator.clipboard.writeText(code);
        const btn = event.target;
        const orig = btn.textContent;
        btn.textContent = '✓';
        setTimeout(() => btn.textContent = orig, 1500);
      } catch (err) {
        console.error('Copy failed:', err);
      }
    };

    // Initialize - if iconName was provided via URL params, load immediately
    if (currentIconName) {
      loadIcon(currentIconName);
    }
  """;
    }
#pragma warning restore MA0051 // Method is too long

    /// <summary>
    /// Builds data for ALL icons so client can look up any icon by name.
    /// </summary>
    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    private static Dictionary<string, object> BuildAllIconsData(IconService iconService, IconSvgProvider svgProvider)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        var allIcons = iconService.GetAllIcons();

        foreach (var icon in allIcons)
        {
            var svgByVariant = BuildSvgByVariant(svgProvider, icon);
            var (defaultVariant, defaultSize) = iconService.GetRecommendedDefault(icon);

            result[icon.Name] = new
            {
                name = icon.Name,
                variants = icon.VariantNames.ToList(),
                sizes = icon.AllSizes.ToList(),
                variantSizes = icon.Variants.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToList(),
                    StringComparer.Ordinal
                ),
                defaultVariant,
                defaultSize,
                svgByVariant
            };
        }

        return result;
    }

    [RequiresUnreferencedCode("This method requires dynamic access to code.")]
    private static Dictionary<string, Dictionary<int, string>> BuildSvgByVariant(IconSvgProvider svgProvider, IconModel icon)
    {
        var result = new Dictionary<string, Dictionary<int, string>>(StringComparer.Ordinal);

        foreach (var kvp in icon.Variants)
        {
            var variantSvgs = new Dictionary<int, string>();

            foreach (var size in kvp.Value)
            {
                var svg = svgProvider.GetSvgContent(icon.Name, kvp.Key, size);
                if (!string.IsNullOrEmpty(svg))
                {
                    variantSvgs[size] = svg;
                }
            }

            if (variantSvgs.Count > 0)
            {
                result[kvp.Key] = variantSvgs;
            }
        }

        return result;
    }

    private static string EscapeJsString(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("'", "\\'", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal);
    }
}
