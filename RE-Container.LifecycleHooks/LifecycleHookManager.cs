using System.Collections.Concurrent;

namespace REContainer.LifecycleHooks;

public sealed class LifecycleHookManager : ILifecycleHookManager
{
	private readonly LifecycleHookHandlerFactory _lifecycleHookHandlerFactory;
	private readonly ConcurrentDictionary<Type, LifecycleHooksHandler> _lifecycleEventHandlers = new();

	public LifecycleHookManager(LifecycleHookHandlerFactory lifecycleHookHandlerFactory)
	{
		_lifecycleHookHandlerFactory = lifecycleHookHandlerFactory;
	}

	public void InstantiateAll()
	{
		InvokeAll<ILifecycleHook>(_ => { });
	}

	public void InvokeAll<TLifecycleHook>(Action<TLifecycleHook> invokeAction) where TLifecycleHook : ILifecycleHook
	{
		var lifecycleHooksHandler = _lifecycleEventHandlers.GetOrAdd(
			typeof(LifecycleHooksHandler<TLifecycleHook>),
			_ => _lifecycleHookHandlerFactory.CreateFromHook(invokeAction));

		lifecycleHooksHandler.InvokeAll();
	}

	public void Dispose()
	{
		_lifecycleHookHandlerFactory.CreateFromHandler<DisposablesHandler>().InvokeAll();
	}
}