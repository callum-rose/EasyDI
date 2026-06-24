using Godot;
using REContainer.Godot.LifetimeScopes;
using REContainer.Registering;

namespace REContainer.Godot.Example.Scripts;

public partial class SessionInstaller : NodeInstaller
{
    public override void Install(IObjectRegistry registry)
    {
        GD.Print("Installing SessionInstaller");
        registry.RegisterSingleton<GameModel>();
    }
}