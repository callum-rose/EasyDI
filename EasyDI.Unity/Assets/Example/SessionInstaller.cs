using EasyDI.LifecycleHooks;
using EasyDI.LifecycleHooks.Games;
using EasyDI.Registering;
using EasyDI.Unity.LifetimeScopes;

namespace EasyDI.Unity.Example
{
	public class SessionInstaller : MonoInstaller
	{
		private class MockGameScopeCreator : IInitialisable
		{
			public void Initialise()
			{
				Instantiate(EasyDISettings.GetScopePrefab<GameLifetimeScope>());
			}
		}

		public override void Install(IObjectRegistry registry)
		{
			registry.RegisterLifecycleHook<MockGameScopeCreator>();
		}
	}
}