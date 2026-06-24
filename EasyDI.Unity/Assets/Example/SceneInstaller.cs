using EasyDI.LifecycleHooks;
using EasyDI.Registering;
using EasyDI.Unity.LifetimeScopes;
using UnityEngine;

namespace EasyDI.Unity.Example
{
	public class SceneInstaller : MonoInstaller
	{
		[SerializeField] private SceneView sceneView = null!;

		public override void Install(IObjectRegistry registry)
		{
			registry.RegisterInstance<ISceneView>(sceneView);
			registry.RegisterLifecycleHook<Presenter>();
		}
	}
}