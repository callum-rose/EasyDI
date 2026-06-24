using System;
using REContainer.LifecycleHooks;
using REContainer.LifecycleHooks.Games;
using REContainer.Registering;
using REContainer.Unity.LifetimeScopes;

namespace REContainer.Unity.Example
{
	public class ApplicationInstaller : MonoInstaller
	{
		private class MockSessionScopeCreator : IInitialisable
		{
			public void Initialise()
			{
				Instantiate(REContainerSettings.SessionLifetimeScope);
			}
		}
		
		public override void Install(IObjectRegistry registry)
		{
			registry.RegisterSingleton<ApplicationService>();
			registry.RegisterLifecycleHook<MockSessionScopeCreator>();
		}
	}
}