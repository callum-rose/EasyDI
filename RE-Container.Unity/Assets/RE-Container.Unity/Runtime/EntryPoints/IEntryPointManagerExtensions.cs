using REContainer.LifecycleHooks;

namespace REContainer.Unity.LifecycleHooks
{
	public static class ILifecycleHookManagerExtensions
	{
		public static void InvokeStartables(this ILifecycleHookManager manager)
		{
			manager.InvokeAll<IStartable>(startable => startable.Start());
		}
	}
}