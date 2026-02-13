# MCP Apps - Interactive UIs for Fluent UI Blazor

This folder contains **MCP Apps** - interactive UI components that render directly in MCP hosts like VS Code, Claude Desktop, and other MCP-enabled clients.

## What are MCP Apps?

MCP Apps extend traditional MCP tools by providing **interactive HTML interfaces** instead of just text responses. They're perfect for:

- 🎨 **Visual exploration** - Browse icons, components, color palettes
- 📝 **Complex forms** - Multi-step configuration with validation
- 📊 **Data visualization** - Charts, graphs, interactive dashboards
- 🖼️ **Media viewers** - PDFs, 3D models, image galleries

## Icon Explorer App

The **Icon Explorer** lets users visually browse, search, and select from the complete Fluent UI icon library.

### Features

- ✅ **Visual Preview** - See icons in real-time with all variants (Filled, Regular, Light, Color)
- 🔍 **Smart Search** - Filter by name with instant results
- 🎛️ **Advanced Filters** - Filter by variant and size
- 📋 **One-Click Copy** - Copy Blazor code instantly
- 🎨 **VS Code Theme Integration** - Matches your editor theme
- 💡 **Interactive Details** - Click any icon for full information

### How It Works

1. **User invokes tool**: `OpenIconExplorer` with optional search/filters
2. **MCP tool declares UI**: Returns metadata with `resourceUri: ui://fluent-icons/explorer.html`
3. **Host renders UI**: Fetches the HTML resource and displays in sandboxed iframe
4. **User interacts**: Searches, filters, clicks icons, copies code
5. **Result sent back**: Selected icon data returned to the conversation

### Architecture

```
┌─────────────────────────────────────┐
│  IconExplorerApp.cs                 │  ← Tool with UI metadata
│  [McpServerTool]                    │
│  [McpMeta("ui", JsonValue = "...")]│
└─────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────┐
│  IconExplorerResource.cs            │  ← Resource handler
│  [McpServerResource]                │
│  UriTemplate = "ui://..."           │
└─────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────┐
│  IconExplorerHtml.cs                │  ← HTML/CSS/JS UI
│  Complete interactive interface     │
└─────────────────────────────────────┘
```

## Testing MCP Apps

MCP Apps require **HTTP transport** for easier local testing (though they can also work with stdio).

### Option 1: HTTP Mode (Recommended for Development)

1. **Switch to HTTP transport**:
   ```bash
   cd src/Tools/McpServer
   mv Program.cs Program.Stdio.cs
   mv Program.Http.cs.example Program.cs
   ```

2. **Run the server**:
   ```bash
   dotnet run
   ```
   Server will listen on `http://localhost:3001/mcp`

3. **Configure VS Code**:
   
   Add to your `.vscode/mcp.json`:
   ```json
   {
     "servers": {
       "fluent-ui": {
         "type": "http",
         "url": "http://localhost:3001/mcp"
       }
     }
   }
   ```

4. **Test the app**:
   - Open Command Palette in VS Code
   - Invoke MCP tool: `OpenIconExplorer`
   - The interactive UI should render in your conversation

### Option 2: Stdio Mode (Production)

MCP Apps also work with stdio transport. The host handles the UI rendering regardless of the transport protocol.

```bash
dotnet run
```

Configure as a stdio MCP server in your MCP host configuration.

## Creating New MCP Apps

Follow this pattern:

### 1. Create the Tool (`Apps/YourApp.cs`)

```csharp
[McpServerToolType]
public class YourApp
{
    [McpServerTool]
    [Description("Opens your interactive app")]
    [McpMeta("ui", JsonValue = """{ "resourceUri": "ui://your-app/index.html" }""")]
    public YourAppResult OpenYourApp(string? parameter = null)
    {
        return new YourAppResult { /* ... */ };
    }
}
```

### 2. Create the Resource (`Resources/YourAppResource.cs`)

```csharp
[McpServerResourceType]
public class YourAppResource
{
    [McpServerResource(
        UriTemplate = "ui://your-app/index.html",
        MimeType = "text/html",
        Title = "Your App UI")]
    public static Task<string> GetYourAppUI()
    {
        return Task.FromResult(YourAppHtml.GetHtml());
    }
}
```

### 3. Create the HTML (`Helpers/YourAppHtml.cs`)

```csharp
public static class YourAppHtml
{
    public static string GetHtml()
    {
        return """
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="UTF-8" />
            <title>Your App</title>
            <style>
                /* Use VS Code theme variables */
                body {
                    background: var(--color-background-primary, #1e1e1e);
                    color: var(--color-text-primary, #cccccc);
                }
            </style>
        </head>
        <body>
            <h1>Your Interactive App</h1>
            
            <script type="module">
                // Send results back to host
                window.parent.postMessage({
                    type: 'mcp-app-result',
                    data: { /* your result */ }
                }, '*');
            </script>
        </body>
        </html>
        """;
    }
}
```

## Best Practices

### Theme Integration

Always use VS Code CSS variables for seamless theme integration:

```css
background: var(--color-background-primary, #1e1e1e);
color: var(--color-text-primary, #cccccc);
border: 1px solid var(--color-border-primary, #3c3c3c);
font-family: var(--font-sans, system-ui);
```

### Communication

Send results back to the host using postMessage:

```javascript
window.parent.postMessage({
    type: 'mcp-app-result',
    data: yourResultData
}, '*');
```

### Code Organization

- **Simple HTML**: Keep in a single file if under 500 lines
- **Complex apps**: Consider using a build system (Vite, webpack) to bundle
- **Shared utilities**: Create helper classes in `Helpers/`

## Resources

- 📖 [MCP Apps Documentation](https://modelcontextprotocol.io/docs/extensions/apps)
- 💻 [Official Examples](https://github.com/modelcontextprotocol/ext-apps)
- 📝 [Bruno's C# Example](https://elbruno.com/2026/01/28/building-an-mcp-app-with-c-a-color-picker-sample/)
- 🎥 [VS Code MCP Apps Video](https://www.youtube.com/watch?v=HWmC3T5Wwqw)

## Troubleshooting

### UI not rendering
- ✅ Check that `resourceUri` in tool matches `UriTemplate` in resource
- ✅ Ensure you're using HTTP transport for local development
- ✅ Verify the HTML is valid and self-contained

### Host not finding the tool
- ✅ Rebuild the project: `dotnet build`
- ✅ Restart the MCP server
- ✅ Check that classes have `[McpServerToolType]` and `[McpServerResourceType]` attributes

### Theme not applying
- ✅ Use CSS variables with fallbacks: `var(--color-text-primary, #cccccc)`
- ✅ Test in both light and dark VS Code themes

## Contributing

When adding new MCP Apps:

1. Follow the folder structure and naming conventions
2. Include comprehensive inline documentation
3. Add usage examples to tool descriptions
4. Test in both light and dark themes
5. Ensure the UI is responsive and accessible
