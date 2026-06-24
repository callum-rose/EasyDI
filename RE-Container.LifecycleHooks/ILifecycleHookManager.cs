namespace REContainer.LifecycleHooks;

public interface ILifecycleHookManager : IDisposable
{
	void InstantiateAll();
	void InvokeAll<TLifecycleHook>(Action<TLifecycleHook> invokeAction) where TLifecycleHook : ILifecycleHook;
}