using EasyDI.LifecycleHooks;

namespace EasyDI.Unity.LifecycleHooks
{
	public interface IStartable : ILifecycleHook
	{
		void Start();
	}
}