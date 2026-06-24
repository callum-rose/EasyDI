# RE-Container Godot Example Project

This example project demonstrates how to use the RE-Container plugin in Godot with C#, featuring a complete MVP (Model-View-Presenter) implementation with dependency injection across multiple lifetime scopes.

> **Note:** For plugin installation, setup instructions, and troubleshooting, see the [Plugin README](addons/Re-Container.Godot/README.md).

## Prerequisites

Before running this example:

1. **Install NuGet Packages**
   - Ensure `RE-Container` and `RE-Container.LifecycleHooks` packages are restored
   - Run `dotnet restore` from the project directory
   - These packages are available from the NuGet source at `T:\NugetTestFolder`

2. **Enable the Plugin**
   - In Godot, go to Project > Project Settings > Plugins
   - Enable "Re-Container Godot"
   - Build the project (Build > Build Solution)

## Project Structure

The example demonstrates a hierarchical dependency injection setup:

### Scenes
- **`ApplicationScope.tscn`** - Root scope, created at application start
- **`SessionScope.tscn`** - Session scope, manually instantiated by MockSessionScopeCreator
- **`MVP Example.tscn`** - Example scene with UI and scene-specific dependencies

### Installers (MonoInstaller scripts)
- **`ApplicationInstaller.cs`** - Registers application-level services (ApplicationService)
- **`SessionInstaller.cs`** - Registers session-level services (GameModel singleton)
- **`SceneInstaller.cs`** - Registers scene-specific services (SceneView, Presenter)

### Configuration
- **`RE Container Settings.tres`** - Resource defining ApplicationScopePrefab and SessionScopePrefab

## Extending the Example

To create your own scenes with dependency injection:

1. **Create a Scene**
   - Design your scene in Godot
   - Add a "Scene Lifetime Scope" node (custom type from plugin)

2. **Create an Installer**
   - Create a new C# script inheriting from `MonoInstaller`
   - Implement the `Install(IObjectRegistry registry)` method
   - Add the installer script as a child node of the SceneLifetimeScope

3. **Configure the Scope**
   - Assign the installer to `PrimaryInstaller` property
   - Set `ParentScopeName` to the appropriate parent scope name
     - Usually `"ApplicationLifetimeScope"` for game scenes
     - Or `"SessionLifetimeScope"` / `"GameLifetimeScope"` if using those

5. **Implement Lifecycle Hooks**
   - Create presenters/services that implement desired interfaces
   - `IInitialisable` - Setup logic
   - `IStartable` - Start logic
   - `ITickable` - Per-frame updates
   - `IPhysicsTickable` - Physics updates
   - `IDisposable` - Cleanup logic

6. **Use Constructor Injection**
   ```csharp
   public class MyPresenter : IInitialisable, IStartable, ITickable, IDisposable
   {
       private readonly IMyView _view;
       private readonly IMyModel _model;
       
       public MyPresenter(IMyView view, IMyModel model)
       {
           _view = view;
           _model = model;
       }
       
       public void Initialise() { /* Setup */ }
       public void Start() { /* Start logic */ }
       public void Tick() { /* Per-frame logic */ }
       public void Dispose() { /* Cleanup */ }
   }
   ```

## Learn More

For complete plugin documentation, installation instructions, and troubleshooting:
- See [Plugin README](addons/Re-Container.Godot/README.md)
- RE-Container core framework documentation
- RE-Container.LifecycleHooks documentation
