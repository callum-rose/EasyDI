using Godot;
using EasyDI.Godot.LifetimeScopes;
using EasyDI.LifecycleHooks;
using EasyDI.Registering;

namespace EasyDI.Godot.Example.Scripts;

public partial class SceneInstaller : NodeInstaller
{
    [Export] private SceneView sceneView = null!;

    public override void Install(IObjectRegistry registry)
    {
        GD.Print("Installing SceneInstaller");
        registry.RegisterInstance<ISceneView>(sceneView);
        registry.RegisterLifecycleHook<Presenter>();
    }
}