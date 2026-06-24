using REContainer.LifecycleHooks;

namespace REContainer.Godot.EntryPoints;

public static class ILifecycleHookManagerExtensions
{
	public static void InvokeStartables(this ILifecycleHookManager manager)
	{
		manager.InvokeAll<IReadyable>(startable => startable.Ready());
	}
}