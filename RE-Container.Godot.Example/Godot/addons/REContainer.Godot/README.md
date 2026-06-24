# RE-Container Godot Plugin

A Godot C# plugin that integrates the RE-Container dependency injection framework with Godot's editor and runtime.

**Author:** Alex Williams  
**Version:** 1.0

## What This Plugin Does

When enabled, the plugin automatically:

1. **Registers Custom Node Types** - Adds specialized lifetime scope nodes to the "Create New Node" dialog
2. **Registers Custom Resources** - Adds RE Container Settings as a custom resource type
3. **Registers Autoload Singletons** - Automatically adds required autoload nodes to your project (DontDestroyOnLoad and Initialiser)

## Installation

1. Reference the RE-Container NuGet packages in your `.csproj` file:
   ```xml
   <ItemGroup>
     <PackageReference Include="RE-Container" Version="x.x.x" />
     <PackageReference Include="RE-Container.LifecycleHooks" Version="x.x.x" />
   </ItemGroup>
   ```
2. Restore NuGet packages from your NuGet source: `dotnet restore`
3. Install the plugin using the zip folder that contains `REContainer.Godot/`
4. Enable the plugin in Project Settings > Plugins
5. Build the project to compile C# scripts

## Usage After Installation

Once the plugin is enabled:

1. **Create RE Container Settings:**
   - Right-click in FileSystem
   - Create New > Resource > RE Container Settings
   - Save as `"RE Container Settings.tres"` in project root

2. **Create Application Scope Scene:**
   - Create new scene uisng `Application Lifetime Scope` node as the root
   ![img_1.png](img_1.png)
   - Create a NodeInstaller script for your application services
   - Attach the installer script as a child node
   - Assign it to the PrimaryInstaller property

3. **Reference in Settings:**
   - Open RE Container Settings resource
   - Assign your Application Scope scene to the ApplicationScopePrefab property

4. **Add Scene Scopes:**
   - In your game scenes, add "Scene Lifetime Scope" nodes
   - Create NodeInstaller scripts for scene-specific services
   - Attach installers as child nodes and assign to PrimaryInstaller
   - Set ParentScopeName to the appropriate parent

5. **Run Your Project:**
   - The Initialiser autoload will automatically create the Application Lifetime Scope
   - Scene scopes will connect to their parent scopes automatically
   - Dependency injection is ready to use!

## Troubleshooting

### Plugin Not Appearing
- Ensure the folder structure is correct: `addons/Re-Container.Godot/`
- Check that `plugin.cfg` exists and is properly formatted
- Try restarting Godot

### Build Errors
- Ensure NuGet packages are restored: `dotnet restore`
- Rebuild the project: `dotnet build` or Build > Build Project in Godot
- Check that RE-Container and RE-Container.LifecycleHooks packages are referenced
- Verify your NuGet source is configured correctly if using a custom feed

### Custom Nodes Not Appearing
- Verify the plugin is enabled in Project > Project Settings > Plugins
- Check the Godot console for error messages during plugin initialization
- Ensure all Runtime scripts are compiled successfully
- Try disabling and re-enabling the plugin

### Autoloads Not Added
- The plugin adds autoloads programmatically - check Project > Project Settings > Autoload
- If manually removed, disable and re-enable the plugin to re-add them
- Do not manually remove DontDestroyOnLoad or Initialiser from autoload

### Duplicate Key Errors on Rebuild
- This can happen during hot reload when scripts are recompiled
- Disable and re-enable the plugin to clear the error
- Restart Godot if the issue persists
- This is a known Godot limitation with C# hot reload