# EasyDI

EasyDI is a minimal and portable dependency injection framework designed for our Unity, Godot and C# projects.

> ⚠️ See [PLUGINS.md](EasyDI.Unity/Assets/EasyDI.Unity/Plugins/PLUGINS.md) for important information about updating this package.

## Why

There are loads of DI frameworks out there. Here are some we've tried:

|                            | Zenject | VContainer | Microsoft DI | Autofac |
|----------------------------|:-------:|:----------:|:------------:|:-------:|
| Supports Unity             |    ✅    |     ✅      |      ⛔       |    ⛔    |
| Supports Godot             |    ⛔    |     ⛔      |      ⛔       |    ⛔    |
| Supports C# Projects       |   〰️    |     ⛔      |      ✅       |    ✅    |
| Simple                     |    ⛔    |     ✅      |      ✅       |    ⛔    |
| Currently Supported        |    ⛔    |     ✅      |      ✅       |    ✅    |
| Container Hierarchies      |    ✅    |     ✅      |      ⛔       |    ✅    |
| Post-Startup Registrations |    ✅    |     ✅      |      ⛔       |    ✅    |

None of these frameworks tick all our boxes. There are plenty more frameworks that aren't listed here, and most would be forgiven for not supporting Unity or Godot if they allowed post-startup registrations or hierarchies (i.e. supports new registrations on scene loads). But very few do because this is a very game-dev-specific use case, where most C# applications only need to register one layer of services at startup.

So we decided to make our own. EasyDI has a similar API to [VContainer](https://github.com/hadashiA/VContainer) but with a leaner feature set and better portability.

# What

- Constructor injection
- Fluent registration API
- Support for transient, scoped and singleton lifetimes
- Instantiation via reflection or factories
- Support for multiple resolvers in a hierarchy
- Can resolve lists of services
- Open generics

## How

### Basic Usage

EasyDI is simple to get up and running

```csharp
var registry = ObjectRegistry.CreateRoot();
registry.RegisterSingleton<ServiceA>();

IObjectResolver resolver = resolverBuilder.Build();

var serviceA = resolver.Resolve<ServiceA>();
```

### Lifetimes

```csharp
registry.RegisterTransient<ServiceA>(); // New instance every time
registry.RegisterScoped<ServiceB>(); // One instance per resolver
registry.RegisterSingleton<ServiceC>(); // One instance shared by the resolver and its children
```

### Instance Providers

```csharp
// Just provides an existing instance
registry.RegisterInstance(new ServiceB());

// Creates a new instance using reflection, where the public constructor with the most resolvable parameters is used
registry.RegisterSingleton<ServiceA>();

// Uses a Func to create the instance
registry.RegisterSingleton<ServiceC>(_ => new ServiceC());
registry.RegisterSingleton<ServiceD>(resolver => new ServiceD(resolver.Resolve<ServiceC>()));
```

### Registering As

_These patterns apply to all lifetimes and instance providers._

You can register a type as an interface.

```csharp
registry.RegisterSingleton<ServiceA>().As<IServiceA>();
// ...
var serviceInterface = resolver.Resolve<IServiceA>();
```

You can register a type as itself.

```csharp
registry.RegisterSingleton<ServiceA>();
// ...
var serviceA = resolver.Resolve<ServiceA>();
```

If you want to register a type as itself and other interfaces, you need to explicitly include itself.

```csharp
registry.RegisterSingleton<ServiceA>()
    .As<ServiceA>()
    .As<IDisposable>();
// ...
var service = resolver.Resolve<ServiceA>();
var disposable = resolver.Resolve<IDisposable>();
```

You can register a type as multiple interfaces.

```csharp
registry.RegisterSingleton<ServiceA>()
    .As<IServiceA>()
    .As<IDisposable>();
// ...
var serviceInterface = resolver.Resolve<IServiceA>();
var disposable = resolver.Resolve<IDisposable>();
```

If you don't know if the actual instance type is assignable to an interface, you can use `TryAs` which will ignore any incompatible types.

```csharp
public class ServiceA : IDisposable { };
public interface IServiceB;
public class ServiceB : IServiceB;
```
```csharp
ServiceA service = GetService();

registry.RegisterInstance(service)
    .TryAs<IServiceB>()
    .TryAs<IDisposable>();

Assert.IsTrue(resolver.CanResolve<IDisposable>());

if (service is IServiceB)
{
    Assert.IsTrue(resolver.CanResolve<IServiceB>());
}
else
{
    Assert.IsFalse(resolver.CanResolve<IServiceB>());
}
```

### Registration Arguments

You can provide specific arguments to be used when constructing an instance. Internally, these are treated the same as any other resolvable type so order doesn't matter. However, because of this they also cannot override existing registrations: see [Duplicate Registrations](#Duplicate-Registrations)

```csharp
public class ServiceA(string Name, IDisposable Disposable) { }
public class DiposableService : IDisposable { }
```
```csharp 
registry.RegisterSingleton<ServiceA>()
    .WithArgument<IDisposable>(new DiposableService())
    .WithArgument("Dennis");
```

### Hierarchies

Resolvers inherit registrations from a parent. Singleton services are shared, scoped are unique per resolver, and transient are always new.

```csharp
var rootRegistry = ObjectRegistry.CreateRoot();
rootRegistry.RegisterSingleton<ServiceA>();
IObjectResolver rootResolver = rootRegistry.Build();
   
var childRegistry = ObjectRegistry.CreateChild(rootResolver);
childRegistry.RegisterSingleton<ServiceB>();
IObjectResolver childResolver = childRegistry.Build();

var serviceAFromRoot = rootResolver.Resolve<ServiceA>();
var canResolveServiceBFromRoot = rootResolver.CanResolve<ServiceB>();

Assert.IsFalse(canResolveServiceBFromRoot);

var serviceAFromChild = childResolver.Resolve<ServiceA>();
var serviceBFromChild = childResolver.Resolve<ServiceB>();

Assert.AreSame(serviceAFromRoot, serviceAFromChild);
```

### Duplicate Registrations

#### In the Same Registry

You can register multiple registrations that are resolvable to the same type. When resolved they're returned in the order they were registered. However, this has some caveats: 

- The resolvable type must be explicitly marked using `MarkResolvableAsMany<T>`, otherwise the registry will throw on build.
- You can no longer resolve the type as a single instance, only as a collection.
- If no registrations are found, an empty list will be returned.

```csharp
public interface IService { }
public class ServiceA : IService { }
public class ServiceB : IService { }
```
```csharp 
registry.MarkResolvableAsMany<IService>();
registry.RegisterSingleton<ServiceA>().As<IService>();
registry.RegisterSingleton<ServiceB>().As<IService>();
// ...
var enumerable = resolver.Resolve<IEnumerable<IService>>(); 
var collection = resolver.Resolve<ICollection<IService>>();
var readonlyList = resolver.Resolve<IReadOnlyList<IService>>();

Assert.That(() => resolver.Resolve<IService>(), Throws.Exception);
```

> If you've explicitly registered an `IReadOnlyList<T>`, `ICollection<T>` or `IEnumerable<T>` (which you shouldn't but) you'll have to resolve that explicitly using `resolver.TryResolve(new SingleInstanceQuery(typeof(IReadOnlyList<T>), out var instance))`.

#### In Hierarchy

To avoid any ambiguity, you cannot have duplicate registrations in a resolver hierarchy. 

The only exception to this is you can register a "many" type in a child resolver that is also "many" in its parent(s). This will only return instances from the local resolver.

```csharp
public interface IService { }
public class ServiceA : IService { }
public class ServiceB : IService { }
public class ServiceC : IService { }
public class ServiceD : IService { }

var rootRegistry = ObjectRegistry.CreateRoot();
rootRegistry.MarkResolvableAsMany<IService>();
rootRegistry.RegisterSingleton<ServiceA>().As<IService>();
rootRegistry.RegisterSingleton<ServiceB>().As<IService>();
IObjectResolver rootResolver = rootRegistry.Build();

var childRegistry = ObjectRegistry.CreateChild(rootResolver);
childRegistry.MarkResolvableAsMany<IService>();
childRegistry.RegisterSingleton<ServiceC>().As<IService>();
childRegistry.RegisterSingleton<ServiceD>().As<IService>();
IObjectResolver childResolver = childRegistry.Build();

var rootServices = rootResolver.Resolve<IEnumerable<IService>>(); // ServiceA, ServiceB
Assert.AreEqual(2, rootServices.Count());

var childServices = childResolver.Resolve<IEnumerable<IService>>(); // ServiceC, ServiceD
Assert.AreEqual(2, childServices.Count()); 
```

### Open Generics

Open generic types can be registered and a closed version of that type can be resolved.

```csharp
public interface ILogger<T> { }
public class Logger<T> : ILogger<T> { }
```
```csharp
registry.RegisterSingleton(typeof(Logger<>));
// ...
var logger = resolver.Resolve<Logger<ServiceA>>();
```
```csharp 
registry.RegisterSingleton(typeof(Logger<>)).As(typeof(ILogger<>));
// ...
var logger = resolver.Resolve<ILogger<ServiceA>>();
```
```csharp
registry.RegisterSingleton(typeof(Logger<>), (resolver, type) => LoggerFactory.Create(type)).As(typeof(ILogger<>));
// ...
var logger = resolver.Resolve<ILogger<ServiceA>>();
```

### Instantiate

You can instantiate types directly via the resolver. This is useful for types that are not registered but need dependencies injected.

```csharp
public class ServiceB(ServiceA ServiceA) { }
```
```csharp
registry.RegisterSingleton<ServiceA>();
// ...
var serviceB = resolver.Instantiate<ServiceB>();
```

### Resolving IObjectResolver

You can resolve the current resolver via `IObjectResolver`. This is useful for factories or types that need to create child resolvers.

```csharp
public class Factory(IObjectResolver Resolver) { }
```
```csharp
var factory = resolver.Resolve<Factory>();

Assert.AreSame(resolver, factory.Resolver);
```

### Resolver Built Callbacks

You can register a callback to be invoked when a resolver is built. This is useful for any logic that requires a finalised dependency graph.

```csharp
registry.RegisterBuildCallback(resolver => 
{
    var service = resolver.Resolve<IService>();
    service.Run();
});
```

## Improvements

- **Better error handling:** if something goes wrong the exceptions thrown could be more descriptive.
- **Circular dependency detection:** currently this will just result in a stack overflow.
- **Compile / Built time checks:** currently everything is resolved at runtime, it would be nice to have some way of checking registrations at compile or build time.
- **Performance:** this probably won't be an issue for us but there are some optimisations that could be made.