using Godot;
using REContainer.Godot.LifetimeScopes;
using REContainer.LifecycleHooks;
using REContainer.LifecycleHooks.Games;
using REContainer.Registering;
namespace REContainer.Godot.Example;

internal partial class ApplicationInstaller : NodeInstaller
{
	private class MockSessionScopeCreator : IInitialisable
	{
		public void Initialise()
		{
			GlobalRoot.InstantiateChild(REContainerSettings.SessionLifetimeScope);
		}
	}
		
	public override void Install(IObjectRegistry registry)
	{
		GD.Print("Installing ApplicationInstaller");
		registry.RegisterSingleton<ApplicationService>();
		registry.RegisterLifecycleHook<MockSessionScopeCreator>();
	}
}