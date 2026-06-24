using REContainer.Registering;
using REContainer.Unity.LifetimeScopes;

namespace REContainer.Unity.Example
{
	public class SessionInstaller : MonoInstaller
	{
		public override void Install(IObjectRegistry registry)
		{
			registry.RegisterSingleton<GameModel>();
		}
	}
}