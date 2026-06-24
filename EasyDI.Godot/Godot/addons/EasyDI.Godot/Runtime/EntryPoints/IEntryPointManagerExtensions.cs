using EasyDI.LifecycleHooks;

namespace EasyDI.Godot.EntryPoints;

public static class ILifecycleHookManagerExtensions
{
	public static void InvokeStartables(this ILifecycleHookManager manager)
	{
		manager.InvokeAll<IReadyable>(startable => startable.Ready());
	}
}