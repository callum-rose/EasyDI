using EasyDI.LifecycleHooks;

namespace EasyDI.Unity.LifecycleHooks
{
	public static class ILifecycleHookManagerExtensions
	{
		public static void InvokeStartables(this ILifecycleHookManager manager)
		{
			manager.InvokeAll<IStartable>(startable => startable.Start());
		}
	}
}