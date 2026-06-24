using EasyDI.Registering;
using EasyDI.Unity.LifetimeScopes;

namespace EasyDI.Unity.Example
{
	public class SessionInstaller : MonoInstaller
	{
		public override void Install(IObjectRegistry registry)
		{
			registry.RegisterSingleton<GameModel>();
		}
	}
}