# RE-Container: Lifecycle Hooks

> Read RE-Container [README.md](../RE-Container/README.md) for information on how the DI framework works.

> ⚠️ This package is now managed via NuGet. Use NuGet package manager to update this package.

## Why

Lifecycle hooks are what they say on the tin: ways to hook your code into your application's lifecycle. 

In Unity lifecycle hooks would include MonoBehaviour events such as `Awake`, `Update` and `OnDestroy`. In Godot they would be `_Ready`, `_Process` and `_ExitTree`. In a WPF application they would be `OnStartup` and `OnExit`.

Having a framework to manage these means:
- Easier separation of our code from an application framework.
- Consistent lifecycle management.
- Easier testing.
- More portable code.

## What

RE-Container has been designed to be as portable as possible across different application types. So it doesn't make any assumptions about how your lifecycle hooks will be invoked. Instead, it lets you register your own and provides a way to invoke them wherever you choose.

## How

### Basic Usage

```csharp
public class MenuPresenter : IInitialisable, IDisposable
{
    public void Initialise() { }
    public void Dispose() { }
}

public class GamePresenter : IInitialisable, ITickable, IDisposable
{
    public void Initialise() { }
    public void Tick() { }
    public void Dispose() { }
}
```
```csharp
registry.RegisterLifecycleHook<GamePresenter>();
registry.RegisterLifecycleHook<MenuPresenter>();
```
```csharp
var lifecycleHookManager = resolver.Resolve<ILifecycleHookManager>();
 
lifecycleHookManager.InvokeAll<IInitialisable>(presenter => presenter.Initialise()); // Calls Initialise on MenuPresenter and GamePresenter
lifecycleHookManager.InvokeAll<ITicktable>(presenter => presenter.Tick()); // Calls Tick on GamePresenter
lifecycleHookManager.Dispose(); // Calls Dispose on GamePresenter and MenuPresenter
```

### Built-In Lifecycle Hooks

- `IInitialisable` - for initialisation logic
- `IDisposable` - for cleanup logic

And some that apply to both Unity and Godot:

- `ITickable` - for per-frame update logic
- `IPhysicsTickable` - for physics update logic

There are some convenient extension methods for invoking some of these built-in lifecycle hooks

```csharp
lifecycleHookManager.InvokeInitialisables();
lifecycleHookManager.InvokeTickables();
lifecycleHookManager.InvokePhysicsTickables();
```

For example, in Unity you could have a MonoBehaviour to invoke the hooks that looks like this:

```csharp
public class LifecycleHookInvoker : MonoBehaviour
{
    private ILifecycleHookManager _lifecycleHookManager;

    private void Awake()
    {
        var registry = ObjectRegistry.CreateRoot();
        registry.RegisterLifecycleHook<ExamplePresenter>();
        registry.RegisterLifecycleHook<ExamplePresenter2>();
        
        IObjectResolver resolver = registry.Build();

        _lifecycleHookManager = resolver.Resolve<ILifecycleHookManager>();
        _lifecycleHookManager.InvokeInitialisables();
    }

    private void Update()
    {
        _lifecycleHookManager.InvokeTickables();
    }

    private void FixedUpdate()
    {
        _lifecycleHookManager.InvokePhysicsTickables();
    }

    private void OnDestroy()
    {
        _lifecycleHookManager.Dispose();
    }
}
```

### Custom Lifecycle Hooks

You can define your own lifecycle hook by creating an interface that implements `ILifecycleHook`, and it'll work just like the built-in ones.

```csharp
public interface IStartable : ILifecycleHook
{
    void Start();
}
```
```csharp
public static class ILifecycleHookManagerExtensions
{
    public static void InvokeStartables(this ILifecycleHookManager lifecycleHookManager)
    {
        lifecycleHookManager.InvokeAll<IStartable>(startable => startable.Start());
    }
}
```
```csharp
lifecycleHookManager.InvokeStartables();
```

### Lifecycle Hooks in Child Resolvers

`ILifecycleHookManager` is registered as scoped so each child resolver can manage its own lifecycle hooks. This is useful, for example, in Unity or Godot scene management where each scene will have its own set of lifecycle hooks.

```csharp
var parentLifecycleHookManager = parentResolver.Resolve<ILifecycleHookManager>();
parentLifecycleHookManager.InvokeInitialisables(); // Invokes initialisables in the parent resolver

var childLifecycleHookManager = childResolver.Resolve<ILifecycleHookManager>();
childLifecycleHookManager.InvokeInitialisables(); // Invokes initialisables in the child resolver. Parent initialisables are not invoked again.

childLifecycleHookManager.Dispose(); // Disposes of disposables in the child resolver. Parent disposables are not disposed.

parentLifecycleHookManager.Dispose(); // Disposes disposables in the parent resolver.
```