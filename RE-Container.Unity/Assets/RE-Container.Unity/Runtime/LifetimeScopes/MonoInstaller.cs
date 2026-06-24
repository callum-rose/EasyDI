using REContainer.Registering;
using UnityEngine;

namespace REContainer.Unity.LifetimeScopes
{
	public abstract class MonoInstaller : MonoBehaviour
	{
		public abstract void Install(IObjectRegistry registry);
	}
}