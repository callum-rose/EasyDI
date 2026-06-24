using EasyDI.Registering;
using UnityEngine;

namespace EasyDI.Unity.LifetimeScopes
{
	public abstract class MonoInstaller : MonoBehaviour
	{
		public abstract void Install(IObjectRegistry registry);
	}
}