namespace REContainer.LifecycleHooks.Games;

public static class ILifecycleHookManagerExtensions
{
	public static void InvokeInitialisables(this ILifecycleHookManager lifecycleHookManager)
	{
		lifecycleHookManager.InvokeAll<IInitialisable>(i => i.Initialise());
	}
	
	public static void InvokeTickables(this ILifecycleHookManager lifecycleHookManager)
	{
		lifecycleHookManager.InvokeAll<ITickable>(t => t.Tick());
	}
	
	public static void InvokePhysicsTickables(this ILifecycleHookManager lifecycleHookManager)
	{
		lifecycleHookManager.InvokeAll<IPhysicsTickable>(ft => ft.PhysicsTick());
	}
}