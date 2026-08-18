using EasyDI.Registering;
using EasyDI.Unity.LifetimeScopes;

namespace EasyDI.Unity.Example
{
	public class GameInstaller : MonoInstaller
	{
		public override void Install(IObjectRegistry registry)
		{
			registry.RegisterSingleton<GameModel>();
		}
	}
}
