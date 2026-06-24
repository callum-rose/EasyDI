using EasyDI.Instantiation;
using EasyDI.Resolving;

namespace EasyDI.LifecycleHooks;

public sealed class LifecycleHookHandlerFactory
{
	private readonly IObjectResolver _objectResolver;

	public LifecycleHookHandlerFactory(IObjectResolver objectResolver)
	{
		_objectResolver = objectResolver;
	}

	public LifecycleHooksHandler CreateFromHook<TLifecycleHook>(Action<TLifecycleHook> invokeAction)
		where TLifecycleHook : ILifecycleHook
	{
		return _objectResolver.TryInstantiate<LifecycleHooksHandler<TLifecycleHook>>(
			out var instance,
			invokeAction.ToArgumentInfo()) ?
			instance :
			NullLifecycleHooksHandler.Instance;
	}

	public LifecycleHooksHandler CreateFromHandler<TLifecycleHooksHandler>()
		where TLifecycleHooksHandler : LifecycleHooksHandler
	{
		return _objectResolver.TryInstantiate<TLifecycleHooksHandler>(out var instance) ?
			instance :
			NullLifecycleHooksHandler.Instance;
	}
}