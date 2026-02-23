# Blazor Hybrid

Host Blazor components in a native macOS WKWebView using the BlazorWebView control.

## Setup

### 1. Add the NuGet Package

```xml
<!-- MyApp.MacOS/MyApp.MacOS.csproj -->
<PackageReference Include="Platform.Maui.MacOS.BlazorWebView" Version="0.2.0-beta.6" />
```

### 2. Register the Handler

```csharp
// MauiProgram.cs
using Microsoft.Maui.Platform.MacOS.Hosting;

public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder
        .UseMauiAppMacOS<MacOSApp>()
        .AddMacOSBlazorWebView();  // Register BlazorWebView handler

    return builder.Build();
}
```

### 3. Link wwwroot Resources

In your macOS app head `.csproj`, link the Blazor static assets:

```xml
<ItemGroup>
  <BundleResource Include="..\MyApp\wwwroot\**"
                  Link="wwwroot\%(RecursiveDir)%(Filename)%(Extension)" />
</ItemGroup>
```

The `wwwroot/` folder should contain your `index.html` and any static assets (CSS, JS, images).

### 4. Use BlazorWebView in a Page

```csharp
using Microsoft.Maui.Platform.MacOS.Controls;

var blazorView = new MacOSBlazorWebView
{
    HostPage = "wwwroot/index.html",
};

blazorView.RootComponents.Add(new RootComponent
{
    Selector = "#app",
    ComponentType = typeof(MyBlazorApp.Main),
});

Content = blazorView;
```

## How It Works

- **WKWebView**: Blazor components are rendered inside a native `WKWebView`
- **Asset loading**: The `MacOSMauiAssetFileProvider` loads files from the app bundle
- **JavaScript interop**: Full Blazor JS interop support via the WebView bridge
- **Threading**: `MacOSBlazorDispatcher` ensures UI updates run on the main AppKit thread

## Conditional Compilation

If your shared project has Blazor pages that should only be available on macOS:

```xml
<!-- In your macOS .csproj -->
<PropertyGroup>
  <DefineConstants>$(DefineConstants);MACAPP</DefineConstants>
</PropertyGroup>
```

```csharp
#if MACAPP
// Register Blazor-specific Shell routes
shell.Items.Add(new ShellContent
{
    Title = "Blazor",
    Route = "blazor",
    ContentTemplate = new DataTemplate(typeof(BlazorPage)),
});
#endif
```

## Debugging with MauiDevFlow

If you have [MauiDevFlow](https://github.com/nicwise/Redth.MauiDevFlow.CLI) set up, you can inspect Blazor WebView content via CDP:

```bash
maui-devflow cdp snapshot           # View DOM as accessible text
maui-devflow cdp Runtime evaluate "document.title"  # Run JS
```
