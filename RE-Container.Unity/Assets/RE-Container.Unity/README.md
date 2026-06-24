# RE-Container: Unity

This package extends RE-Container to support Unity projects. 

> See [RE-Container](../../../RE-Container/README.md) and [RE-Container: Lifecycle Hooks](../../../RE-Container.LifecycleHooks/README.md) for information on how the DI framework and lifecycle hooks work.

> ⚠️ See [PLUGINS.md](Plugins/PLUGINS.md) for important information about using this package.

## Example

An example scene is included, so take a look there as it's the best way to see how it all fits together.

## What

This introduces `LifetimeScope` to register services from installers, build the object resolver and invoke any entry points. This has been written in a fairly opinionated way since our application structures in FC and ES are similar.

A `LifetimeScope` does these things in this order:

- `Awake`
  - It looks for a parent scope and if found uses its `IObjectResolver` as the parent resolver
  - It runs any installers assigned to it to register services
  - It builds the `IObjectResolver`
  - `IInitialisable`s are invoked
- `Start` invokes all `IStartable`s
- `Update` invokes all `ITickable`s
- `FixedUpdate` invokes all `IPhysicsTickable`s
- `OnDestroy` invokes all `IDisposable`s

> *`IStartable` is a new entry point defined for Unity's `Start` event

There are 4 pre-defined `LifetimeScope` types

### `ApplicationLifetimeScope`

The root scope that is created automatically when the application starts and destroyed when it ends. It should be used to register services that will live for the entire application lifetime. Only one of these should exist at runtime.

### `SessionLifetimeScope`

Should be created when a session (in the FC and ES sense) starts. It will parent to the `ApplicationLifetimeScope` and should register services that will live for the duration of a session. Only one of these should exist at runtime. This is predefined because I imagine most of our Unity apps will have a session concept.

### `GameLifetimeScope`

Should be created when a game starts. It will parent to the `SessionLifetimeScope` and should register services that will live for the duration of a game. Only one of these should exist at runtime. This is predefined because I imagine most of our Unity apps will have a game concept; an exception being a leaderboard, perhaps.

### `SceneLifetimeScope`

Should be one in each scene. It's resolver will parent to whichever of the aforementioned is set and should register services that will live for the duration of the scene. There should be one of these per scene.

## How

### Scripts

- Create a `MonoInstaller` class for the application scope.
- Create a `MonoInstaller` class for a scene scope.
- Optionally create a `MonoInstaller` script for the session scope if you want to use it.
- Optionally create a `MonoInstaller` script for the game scope if you want to use it.

### Assets

- Create a new GameObject and add the `ApplicationLifetimeScope` and application-installer components to it. Make it a prefab.
- Optionally repeat the above for the `SessionLifetimeScope` and `GameLifetimeScope` components.
- Create a `REContainerSettings` scriptable object and put it in a _Resources_ folder. Assign the application-lifetime-scope prefab to it, and the session-lifetime-scope and game-lifetime-scope prefabs if you created them.
- Now if you run the game, the application-lifetime-scope will be created automatically and put in _DontDestroyOnLoad_.
- Session-lifetime-scope and game-lifetime-scope need to be created and destroyed at runtime manually, in line with a session and a game's respective start and end. You can do this in a few ways:

  - Instantiate the prefabs referenced in `REContainerSettings.SessionLifetimeScope` and `REContainerSettings.GameLifetimeScope`.
  - Instantiate the prefabs yourself.
  - Create new GameObjects, add the relevant lifetime scope component, and use `LifetimeScope.EnqueueInstaller` to set the installer.

> Scene-lifetime and game-lifetime scopes will be automatically parented to their parent scope to ensure cascading destruction.
 
- Create / open a scene and add a `SceneLifetimeScope` component to a game object. Add the scene-installer component to it. Set the parent.
- Run the game again, and the scene-lifetime-scope will have access to all the services registered in its parent hierarchy.

### MonoInstallers

These are how the lifetime scopes register services

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