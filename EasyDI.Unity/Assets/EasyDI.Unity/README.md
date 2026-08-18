# EasyDI: Unity

This package extends EasyDI to support Unity projects. 

> See [EasyDI](../../../EasyDI/README.md) and [EasyDI: Lifecycle Hooks](../../../EasyDI.LifecycleHooks/README.md) for information on how the DI framework and lifecycle hooks work.

> ⚠️ This package is self-contained — the EasyDI DLLs it needs are vendored in
> [`Runtime/Plugins`](Runtime/Plugins/VENDORED.md), so there's nothing else to install.

## Installing

In Unity, _Window ▸ Package Manager ▸ + ▸ Install package from git URL_, and give it:

```
https://github.com/callum-rose/EasyDI.git?path=EasyDI.Unity/Assets/EasyDI.Unity#EasyDI.Unity-v1.0.0
```

Both halves of that URL matter. `?path=` is there because the package sits in a subfolder rather
than at the repo root, and needs Unity 2019.3.4f1 or newer. The `#` fragment pins a release: drop
it and you track the default branch instead, so the same URL resolves to different code over time.

To upgrade, edit the tag in the URL. UPM has no version ranges for git dependencies — every
install is a fixed snapshot, and [the tags](https://github.com/callum-rose/EasyDI/tags) named
`EasyDI.Unity-v*` are the releases of this package.

## Example

An example scene is included, so take a look there as it's the best way to see how it all fits together. It declares a three-layer chain — application, session, game — as example scripts, each scope instantiating the next from the settings asset, with the scene's `SceneLifetimeScope` nested inside the game scope. Its presenter resolves the application scope's timer, the game scope's model and the scene scope's view, so you can watch resolution run the length of the chain. That's one project's structure, not the package's.

## What

This introduces `LifetimeScope` to register services from installers, build the object resolver and invoke any entry points.

A `LifetimeScope` does these things in this order:

- `Awake`
  - It looks for a parent scope and if found uses its `IObjectResolver` as the parent resolver
  - It runs the installer enqueued with `LifetimeScope.EnqueueInstaller`, if there is one
  - It runs the installer assigned to its _Installer_ field, if there is one
  - It runs `Configure`, which subclasses override to register services from code
  - It builds the `IObjectResolver`
  - `IInitialisable`s are invoked
- `Start` invokes all `IStartable`s
- `Update` invokes all `ITickable`s
- `FixedUpdate` invokes all `IPhysicsTickable`s
- `OnDestroy` invokes all `IDisposable`s

> *`IStartable` is a new entry point defined for Unity's `Start` event

## Declaring your layers

How many scopes you have and how they nest is up to your project; the package doesn't ship a structure. A layer is a `LifetimeScope` subclass, and it declares its place in the chain through the base class it derives from:

```csharp
// The top of the chain. Instantiated automatically before the first scene loads.
public sealed class ApplicationLifetimeScope : RootLifetimeScope
{
}

// Nested inside the application scope.
public sealed class SessionLifetimeScope : LifetimeScope<ApplicationLifetimeScope>
{
}

// Nested inside the session scope. Chains can be as deep as you like.
public sealed class GameLifetimeScope : LifetimeScope<SessionLifetimeScope>
{
}
```

Services registered in a scope are resolvable from the scopes nested inside it, but not the other way around.

**Assets → Create → EasyDI** has a template for each: _Root Lifetime Scope Script_ and _Child Lifetime Scope Script_. The child template deliberately doesn't compile until you replace its `TODO_ParentScopeType` placeholder with the scope you're nesting inside.

Only one instance of a given scope type is expected at runtime: a scope finds its parent by searching the whole scene with `FindObjectsByType` and taking the first match. A scope whose parent isn't there becomes a root scope of its own rather than failing, which is what lets a scene run on its own.

### `SceneLifetimeScope`

The one concrete scope the package ships. Put one in each scene and pick its parent from the dropdown in its inspector, which lists every scope type in the project. It doesn't reparent its transform, since it belongs to its scene, and its _Testing Backup Installer_ runs only when the chosen parent scope isn't present — so a scene can substitute the services it would otherwise have inherited and run on its own.

Its parent is stored as a name rather than a type, so a scope that's since been renamed or deleted shows up as an error in the inspector, and entering the scene fails fast rather than quietly running without its parent's services.

### Overriding `ParentScopeType` directly

`RootLifetimeScope` and `LifetimeScope<TParent>` are thin: each only overrides `ParentScopeType` on `LifetimeScope`, returning `null` or `typeof(TParent)`. Override it yourself when a scope has to decide its parent from its own state, as `SceneLifetimeScope` does. From an assembly other than this one, declare the override `protected` — the base member is `protected internal`, and the `internal` half isn't visible to you:

```csharp
protected override Type? ParentScopeType => _isNested ? typeof(SomeOtherScope) : null;
```

## Settings

`EasyDISettings` is a `ScriptableObject`. Create one via **Assets → Create → EasyDI → Settings**, and keep exactly one in the project; it's added to the player's _Preloaded Assets_ for you. It holds:

- **Root Lifetime Scope** — the prefab instantiated before the first scene loads and put in _DontDestroyOnLoad_. It has to be a scope with no parent.
- **Scope Prefabs** — the prefabs you want to look up by type at runtime:

```csharp
var sessionScope = Instantiate(EasyDISettings.GetScopePrefab<SessionLifetimeScope>());

if (EasyDISettings.TryGetScopePrefab<GameLifetimeScope>(out var gameScopePrefab))
{
    // ...
}
```

`GetScopePrefab<T>` searches the root scope as well as the list, and throws when nothing matches or when more than one prefab would. `TryGetScopePrefab<T>` returns false instead when nothing matches, but still throws on an ambiguous match.

The settings inspector finds the scope prefabs in your project: it offers a dropdown for the root, an _Auto-populate_ button for the list, a button to create a prefab for any scope type that hasn't got one, and it flags empty, duplicated and misplaced entries.

Every scope but the root is yours to create and destroy in line with whatever it represents. You can:

- Instantiate a prefab from `EasyDISettings.GetScopePrefab<T>()`.
- Instantiate a prefab yourself.
- Create a GameObject, add the scope component, and use `LifetimeScope.EnqueueInstaller` to set the installer.

> A scope is parented to its parent scope's transform, so destroying a scope destroys everything nested inside it. `SceneLifetimeScope` is the exception, staying where it is in its scene.

## Getting started

- Create a root scope script and a `MonoInstaller` for it.
- Create a GameObject, add both components to it, and make it a prefab.
- Create an `EasyDISettings` asset and assign that prefab as its root scope.
- Add a `SceneLifetimeScope` and a scene `MonoInstaller` to a scene, and choose the parent scope in the scope's inspector.
- Run the game: the root scope is instantiated before the scene loads, and the scene scope resolves everything registered above it.
- Add layers in between as you need them, deriving each from `LifetimeScope<TParent>` and registering its prefab in the settings asset.

## MonoInstallers

These are how the lifetime scopes register services. Assign one to a scope's _Installer_ field, or register from code by overriding the scope's `Configure`.

```csharp
public class GameInstaller : MonoInstaller
{
    public override void Install(IObjectRegistry registry)
    {
        registry.RegisterSingleton<IGameService, GameService>();
        registry.RegisterTransient<VideoPlayer>(Factory);
    }
    
    private VideoPlayer Factory(IObjectResolver resolver)
    {
        // ...
    }
}
```
