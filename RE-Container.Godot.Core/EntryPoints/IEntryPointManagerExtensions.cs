using REContainer.LifecycleHooks;

namespace REContainer.Godot.EntryPoints;

public static class ILifecycleHookManagerExtensions
{
	public static void InvokeReadyables(this ILifecycleHookManager manager)
	{
		manager.InvokeAll<IReadyable>(startable => startable.Ready());
	}
}