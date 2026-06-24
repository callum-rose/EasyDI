using System;
using EasyDI.LifecycleHooks;
using EasyDI.LifecycleHooks.Games;
using EasyDI.Registering;
using EasyDI.Unity.LifetimeScopes;

namespace EasyDI.Unity.Example
{
	public class ApplicationInstaller : MonoInstaller
	{
		private class MockSessionScopeCreator : IInitialisable
		{
			public void Initialise()
			{
				Instantiate(EasyDISettings.SessionLifetimeScope);
			}
		}
		
		public override void Install(IObjectRegistry registry)
		{
			registry.RegisterSingleton<ApplicationService>();
			registry.RegisterLifecycleHook<MockSessionScopeCreator>();
		}
	}
}