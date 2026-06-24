# EasyDI: Configuration

## Why

Modern .NET applications often use `Microsoft.Extensions.Configuration` and `Microsoft.Extensions.Options` for managing
application settings.

## What

This extension brings that familiar configuration pattern to EasyDI, allowing you to:

- Bind configuration sections to strongly-typed options classes.
- Validate options using data annotations.
- Use the standard `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>` patterns.

## How

### Basic Usage

```json
// appsettings.json
{
  "GameSettings": {
    "PlayerName": "Player1",
    "MaxPlayers": 16
  },
  "BackendSettings": {
    "Server": {
      "Host": "localhost",
      "Port": 7777
    }
  }
}
```

```csharp
public class GameSettings
{
    [Required]
    public string PlayerName { get; set; }
    
    // No validation attributes, uses default value if not set
    public int MaxPlayers { get; set; } = 16;
}

public class ServerSettings
{
    [Required]
    public string Host { get; set; }
        
    [Required, Range(1, 65535)]
    public int Port { get; set; }
}
```

```csharp
IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

registry.RegisterRootConfiguration(configuration);
registry.RegisterOptions<GameSettings>("GameSettings");
registry.RegisterOptions<ServerSettings>("BackendSettings:Server");
```

```csharp
var gameOptions = resolver.Resolve<IOptions<GameSettings>>();
var serverOptions = resolver.Resolve<IOptions<ServerSettings>>();
```

### Validation

Options are validated using data annotations when resolved. If validation fails, an `OptionsValidationException` is
thrown. You can force validation at resolver build time using:

```csharp
registry.RegisterOptions<GameSettings>("GameSettings").ValidateOnBuild();
```

### Options Patterns

All three standard options interfaces are supported:

```csharp
var options = resolver.Resolve<IOptions<GameSettings>>(); // Singleton that does not reload
var snapshot = resolver.Resolve<IOptionsSnapshot<GameSettings>>(); // Scoped that recomputes per scope
var monitor = resolver.Resolve<IOptionsMonitor<GameSettings>>(); // Singleton that supports reloading
```

### Configuration in Child Resolvers

`RegisterRootConfiguration` should be called on the root registry. Child registries don't need to call this again and
can then register options as before:

```csharp
parentRegistry.RegisterRootConfiguration(configuration);
// ..
childRegistry.RegisterOptions<LevelSettings>("LevelSettings");
```

