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

	public LifecycleHooksHandler<TLifecycleHook> CreateFromHook<TLifecycleHook>()
		where TLifecycleHook : ILifecycleHook
	{
		return _objectResolver.TryInstantiate<LifecycleHooksHandler<TLifecycleHook>>(out var instance) ?
			instance :
			new LifecycleHooksHandler<TLifecycleHook>([]);
	}

	public LifecycleHooksHandler CreateFromHandler<TLifecycleHooksHandler>()
		where TLifecycleHooksHandler : LifecycleHooksHandler
	{
		return _objectResolver.TryInstantiate<TLifecycleHooksHandler>(out var instance) ?
			instance :
			NullLifecycleHooksHandler.Instance;
	}
}