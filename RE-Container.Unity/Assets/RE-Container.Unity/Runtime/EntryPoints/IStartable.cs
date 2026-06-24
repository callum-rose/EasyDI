using REContainer.LifecycleHooks;

namespace REContainer.Unity.LifecycleHooks
{
	public interface IStartable : ILifecycleHook
	{
		void Start();
	}
}