using Godot;
using EasyDI.Godot.LifetimeScopes;
using EasyDI.LifecycleHooks;
using EasyDI.LifecycleHooks.Games;
using EasyDI.Registering;
namespace EasyDI.Godot.Example;

internal partial class ApplicationInstaller : NodeInstaller
{
	private class MockSessionScopeCreator : IInitialisable
	{
		public void Initialise()
		{
			GlobalRoot.InstantiateChild(EasyDISettings.SessionLifetimeScope);
		}
	}
		
	public override void Install(IObjectRegistry registry)
	{
		GD.Print("Installing ApplicationInstaller");
		registry.RegisterSingleton<ApplicationService>();
		registry.RegisterLifecycleHook<MockSessionScopeCreator>();
	}
}