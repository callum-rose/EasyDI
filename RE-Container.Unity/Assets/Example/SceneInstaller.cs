using REContainer.LifecycleHooks;
using REContainer.Registering;
using REContainer.Unity.LifetimeScopes;
using UnityEngine;

namespace REContainer.Unity.Example
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