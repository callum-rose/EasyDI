using Godot;
using REContainer.Godot.LifetimeScopes;
using REContainer.LifecycleHooks;
using REContainer.Registering;

namespace REContainer.Godot.Example.Scripts;

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