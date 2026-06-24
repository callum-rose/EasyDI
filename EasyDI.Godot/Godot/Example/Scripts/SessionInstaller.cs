using Godot;
using EasyDI.Godot.LifetimeScopes;
using EasyDI.Registering;
namespace EasyDI.Godot.Example;

public partial class SessionInstaller : NodeInstaller
{
    public override void Install(IObjectRegistry registry)
    {
        GD.Print("Installing SessionInstaller");
        registry.RegisterSingleton<GameModel>();
    }
}